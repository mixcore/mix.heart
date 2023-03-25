using Microsoft.Extensions.Caching.Distributed;
using Mix.Heart.Entities.Cache;
using Mix.Heart.Enums;
using Mix.Heart.Models;
using Mix.Heart.Repository;
using Mix.Heart.UnitOfWork;
using Mix.Shared.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using StackExchange.Redis;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Mix.Heart.Services
{
    public class MixCacheService
    {
        private readonly MixDitributedCache _ditributedCache;
        private readonly MixHeartConfigurationModel _configs;
        private readonly EntityRepository<MixCacheDbContext, MixCache, Guid> _repository;
        public bool IsCacheEnabled { get => _configs.IsCache; }
        protected JsonSerializer serializer;

        public MixCacheService()
        {
            _configs = MixHeartConfigService.Instance.AppSettings;
            if (_configs.CacheMode == MixCacheMode.DATABASE)
            {
                var uow = new UnitOfWorkInfo(new MixCacheDbContext());
                _repository = new(uow);
            }
            serializer = new JsonSerializer()
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                NullValueHandling = NullValueHandling.Ignore
            };
            serializer.Converters.Add(new StringEnumConverter());
        }
        
        public MixCacheService(MixDitributedCache ditributedCache)
        {
            _configs = MixHeartConfigService.Instance.AppSettings;
            if (_configs.CacheMode == MixCacheMode.DATABASE)
            {
                var uow = new UnitOfWorkInfo(new MixCacheDbContext());
                _repository = new(uow);
            }
            serializer = new JsonSerializer()
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                NullValueHandling = NullValueHandling.Ignore
            };
            serializer.Converters.Add(new StringEnumConverter());
            _ditributedCache = ditributedCache;
        }

       
        #region Get

        public Task<T> GetAsync<T>(string key, string cacheFolder, string filename, CancellationToken cancellationToken = default)
            where T: class
        {
            try
            {
                switch (_configs.CacheMode)
                {
                    case MixCacheMode.DATABASE:
                        if (_ditributedCache != null)
                        {
                            return _ditributedCache.GetFromCache<T>($"{cacheFolder}_{key}_{filename}");
                        }
                        return GetFromDatabaseAsync<T>(key, cacheFolder, filename, cancellationToken);
                    case MixCacheMode.JSON:
                    default:
                        return Task.FromResult(GetFromJson<T>(key, cacheFolder, filename));
                }
            }
            catch (Exception ex)
            {
                //TODO Handle Exception
                Console.WriteLine(ex);
                return Task.FromResult(default(T));
            }
        }

        private T GetFromJson<T>(string key, string folder, string filename)
        {
            string filePath = $"{_configs.CacheFolder}/{folder}/{key}/{filename}.json";
            if (File.Exists(filePath))
            {
                using (StreamReader file = File.OpenText(filePath))
                using (JsonTextReader reader = new JsonTextReader(file))
                {
                    try
                    {
                        JObject jobj = (JObject)JToken.ReadFrom(reader);
                        return jobj.ToObject<T>();
                    }
                    catch (Exception ex)
                    {
                        Console.Write(ex);
                        return default;
                    }
                }
            }
            return default;
        }

        private async Task<T> GetFromDatabaseAsync<T>(string key, string folder, string filename, CancellationToken cancellationToken = default)
        {
            try
            {
                string keyword = $"{folder}_{key}_{filename}";
                var cache = await _repository.GetSingleAsync(m => m.Keyword == keyword, cancellationToken);
                if (cache != null)
                {
                    try
                    {
                        JObject jobj = JObject.Parse(cache.Value);
                        return jobj.ToObject<T>();
                    }
                    catch (Exception ex)
                    {
                        Console.Write(ex);
                        return default;
                    }
                }
                return default;
            }
            catch (Exception ex)
            {
                //TODO Handle Exception
                Console.WriteLine(ex);
                return default;
            }
        }
        #endregion

        #region Set
        
        public async Task SetAsync<T>(string key, T value, string cacheFolder, string filename, CancellationToken cancellationToken = default)
            where T : class
        {
            if (value != null)
            {
                switch (_configs.CacheMode)
                {
                    case MixCacheMode.DATABASE:
                        if (_ditributedCache != null)
                        {
                            await _ditributedCache.SetCache($"{cacheFolder}_{key}_{filename}", value, new()
                            {
                                SlidingExpiration = TimeSpan.FromMinutes(_configs.SlidingExpirationInMinute)
                            });
                        }
                        await SaveDatabaseAsync(key, value, cacheFolder, filename, cancellationToken);
                        break;
                    case MixCacheMode.JSON:
                    default:
                        SaveJson(key, value, cacheFolder, filename);
                        break;
                }
            }
        }

        private bool SaveJson<T>(string key, T value, string cacheFolder, string filename)
        {
            try
            {
                var jobj = JObject.FromObject(value, serializer);

                var cacheFile = new FileModel()
                {
                    Filename = filename.ToLower(),
                    Extension = ".json",
                    FileFolder = $"{_configs.CacheFolder}/{cacheFolder}/{key.ToLower()}",
                    Content = jobj.ToString(Formatting.None)
                };
                return MixFileHelper.SaveFile(cacheFile) != null;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return false;
            }
        }

        private async Task<bool> SaveDatabaseAsync<T>(string key, T value, string cacheFolder, string filename, CancellationToken cancellationToken = default)
        {
            if (value != null)
            {
                var jobj = JObject.FromObject(value);

                var cache = new MixCache()
                {
                    Id = Guid.NewGuid(),
                    Keyword = $"{cacheFolder}_{key}_{filename}",
                    Value = jobj.ToString(Formatting.None),
                    CreatedDateTime = DateTime.UtcNow
                };

                await _repository.SaveAsync(cache, cancellationToken);
                await _repository.UowInfo.CompleteAsync(cancellationToken);
            }
            return true;
        }

        #endregion

        #region Clear

        public async Task RemoveCacheAsync(CancellationToken cancellationToken = default)
        {
            switch (_configs.CacheMode)
            {
                case MixCacheMode.DATABASE:
                    await _repository.DeleteManyAsync(m => true, cancellationToken);
                    break;
                case MixCacheMode.JSON:
                default:
                    MixFileHelper.EmptyFolder(_configs.CacheFolder);
                    break;
            }
        }

        public async Task RemoveCacheAsync(object key, string cacheFolder, CancellationToken cancellationToken = default)
        {
            string keyPath = $"{cacheFolder}_{key}";
            if (_ditributedCache != null)
            {
                await _ditributedCache.ClearCache(keyPath);
            }
            else
            {
                switch (_configs.CacheMode)
                {
                    case MixCacheMode.DATABASE:
                        await _repository.DeleteAsync(m => m.Keyword == keyPath, cancellationToken);
                        break;
                    case MixCacheMode.JSON:
                    default:
                        MixFileHelper.DeleteFolder(
                            $"{_configs.CacheFolder}/{cacheFolder}/{key}");
                        break;
                }
            }
        }
        #endregion
    }
}