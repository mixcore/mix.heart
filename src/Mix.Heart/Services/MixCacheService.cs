using Microsoft.Extensions.Configuration;
using Mix.Heart.Entities.Cache;
using Mix.Heart.Enums;
using Mix.Heart.Model;
using Mix.Heart.Models;
using Mix.Heart.Repository;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Mix.Heart.Services
{
    public class MixCacheService
    {
        private readonly IConfiguration _configuration;
        private readonly MixFileService _fileService;
        private readonly MixHeartConfigurationModel _configs = new MixHeartConfigurationModel();
        private readonly EntityRepository<MixCacheDbContext, MixCache, string> _repository;
        public bool IsCacheEnabled { get => _configs.IsCache; }
        public MixCacheService(
            IConfiguration configuration,
            MixFileService fileService,
            EntityRepository<MixCacheDbContext, MixCache, string> repository)
        {
            _configuration = configuration;
            _fileService = fileService;
            _configuration.GetSection("MixHeart").Bind(_configs);
            _repository = repository;
        }

        static MixCacheService()
        {
        }

        private bool SaveJson<T>(string key, T value, Type dataType)
        {
            try
            {
                var jobj = JObject.FromObject(value);

                var cacheFile = new FileModel()
                {
                    Filename = key.ToLower(),
                    Extension = ".json",
                    FileFolder = $"{_configs.CacheFolder}/{dataType.FullName}",
                    Content = jobj.ToString(Formatting.None)
                };
                return MixFileService.Instance.SaveFile(cacheFile);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return false;
            }
        }


        public Task<T> GetAsync<T>(string key, Type dataType)
        {
            try
            {
                switch (_configs.CacheMode)
                {
                    case MixCacheMode.DATABASE:
                        return GetFromDatabaseAsync<T>(key, dataType.FullName);
                    case MixCacheMode.JSON:
                    default:
                        return Task.FromResult(GetFromJson<T>(key, dataType.FullName));
                }
            }
            catch (Exception ex)
            {
                //TODO Handle Exception
                Console.WriteLine(ex);
                return Task.FromResult(default(T));
            }
        }

        private T GetFromJson<T>(string key, string folder = null)
        {
            string filePath = $"{_configs.CacheFolder}/{folder}/{key}.json";
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

        private async Task<T> GetFromDatabaseAsync<T>(string key, string folder)
        {
            try
            {
                string id = $"{folder}/{key}";
                var cache = await _repository.GetByIdAsync(id);
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

        public async Task<bool> SetAsync<T>(string key, T value, Type dataType)
        {
            if (value != null)
            {
                switch (_configs.CacheMode)
                {
                    case MixCacheMode.DATABASE:
                        await SaveDatabaseAsync(key, value, dataType);
                        break;
                    case MixCacheMode.JSON:
                    default:
                        SaveJson(key, value, dataType);
                        break;
                }
            }
            return true;
        }

        private async Task<bool> SaveDatabaseAsync<T>(string key, T value, Type dataType)
        {
            if (value != null)
            {
                var jobj = JObject.FromObject(value);

                var cache = new MixCache()
                {
                    Id = $"{dataType.FullName}_{key}",
                    Value = jobj.ToString(Formatting.None),
                    CreatedDateTime = DateTime.UtcNow
                };

                await _repository.SaveAsync(cache);
            }
            return true;
        }

        public async Task RemoveCacheAsync()
        {
            switch (_configs.CacheMode)
            {
                case MixCacheMode.DATABASE:
                    await _repository.DeleteManyAsync(m => true);
                    break;
                case MixCacheMode.JSON:
                default:
                    _fileService.EmptyFolder(_configs.CacheFolder);
                    break;
            }
        }

        public async Task RemoveCacheAsync(string key, Type dataType)
        {
            switch (_configs.CacheMode)
            {
                case MixCacheMode.DATABASE:
                    await _repository.DeleteAsync(m => m.Id == $"{dataType.FullName}_{key}");
                    break;
                case MixCacheMode.JSON:
                default:
                    _fileService.DeleteFile(
                        $"{_configs.CacheFolder}/{dataType.FullName}/{key}.json");
                    break;
            }
        }
    }
}