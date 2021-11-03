using Microsoft.EntityFrameworkCore;
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
        private readonly MixHeartConfigurationModel _configs;
        private readonly EntityRepository<MixCacheDbContext, MixCache, string> _repository;

        public MixCacheService(
            IConfiguration configuration,
            MixFileService fileService,
            EntityRepository<MixCacheDbContext, MixCache, string> repository)
        {
            _configuration = configuration;
            _fileService = fileService;
            _configs = JObject.Parse(
                        _configuration.GetSection("MixHeart").Value)
                        .ToObject<MixHeartConfigurationModel>();
            _repository = repository;
        }

        static MixCacheService()
        {
        }

       
        private bool SaveJson<T>(string key, T value, string folder)
        {
            try
            {
                var jobj = JObject.FromObject(value);

                var cacheFile = new FileModel()
                {
                    Filename = key.ToLower(),
                    Extension = ".json",
                    FileFolder = $"{_configs.CacheFolder}/{folder}",
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


        public Task<T> GetAsync<T>(string key, string folder = null)
        {
            try
            {
                switch (_configs.CacheMode)
                {
                    case MixCacheMode.DATABASE:
                        return GetFromDatabaseAsync<T>(key, folder);
                    case MixCacheMode.JSON:
                    default:
                        return Task.FromResult(GetFromJson<T>(key, folder));
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

        public async Task<bool> SetAsync<T>(string key, T value, string folder = null)
        {
            if (value != null)
            {
                switch (_configs.CacheMode)
                {
                    case MixCacheMode.DATABASE:
                        await SaveDatabaseAsync(key, value, folder);
                        break;
                    case MixCacheMode.JSON:
                    default:
                        SaveJson(key, value, folder);
                        break;
                }
            }
            return true;
        }

        private async Task<bool> SaveDatabaseAsync<T>(string key, T value, string folder)
        {
            if (value != null)
            {
                var jobj = JObject.FromObject(value);

                var cache = new MixCache()
                {
                    Id = $"{folder}/{key}",
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

        public async Task RemoveCacheAsync(string folder)
        {
            switch (_configs.CacheMode)
            {
                case MixCacheMode.DATABASE:
                    await _repository.DeleteManyAsync(m => EF.Functions.Like(m.Id, $"%{folder}%"));
                    break;
                case MixCacheMode.JSON:
                default:
                    _fileService.DeleteFolder(
                        $"{_configs.CacheFolder}/{folder}");
                    break;
            }
        }

        public Task RemoveCacheAsync(Type type, string key = null)
        {
            string path = $"{_configs.CacheFolder}/{type.FullName}/";
            if (!string.IsNullOrEmpty(key))
            {
                path += $"/{key}";
            }
            return Task.FromResult(_fileService.EmptyFolder(path));
        }
    }
}