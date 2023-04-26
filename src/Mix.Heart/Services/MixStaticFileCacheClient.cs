using Mix.Heart.Interfaces;
using Mix.Heart.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Mix.Heart.Services
{
    public class MixStaticFileCacheClient : IDitributedCacheClient
    {
        private readonly string _cacheFolder;
        private readonly JsonSerializer serializer;
        public MixStaticFileCacheClient(string cacheFolder)
        {
            _cacheFolder = cacheFolder;
            serializer = new JsonSerializer()
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                NullValueHandling = NullValueHandling.Ignore
            };
            serializer.Converters.Add(new StringEnumConverter());
        }

        public async Task<T> GetFromCache<T>(string key, CancellationToken cancellationToken = default) where T : class
        {
            string filename = key.Substring(key.LastIndexOf('/') + 1);
            string folder = key.Substring(0, key.LastIndexOf('/'));
            string filePath = $"{_cacheFolder}/{folder}/{filename}.json";
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

        public Task SetCache<T>(string key, T value, CancellationToken cancellationToken = default) where T : class
        {
            try
            {
                var jobj = JObject.FromObject(value, serializer);
                string filename = key.Substring(key.LastIndexOf('/') + 1);
                string folder = key.Substring(0, key.LastIndexOf('/'));
                var cacheFile = new FileModel()
                {
                    Filename = filename.ToLower(),
                    Extension = ".json",
                    FileFolder = $"{_cacheFolder}/{folder}",
                    Content = jobj.ToString(Formatting.None)
                };
                MixFileHelper.SaveFile(cacheFile);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return Task.CompletedTask;
        }

        public Task ClearCache(string key, CancellationToken cancellationToken = default)
        {
            MixFileHelper.DeleteFolder($"{_cacheFolder}/{key}");
            return Task.CompletedTask;
        }

        public Task ClearAllCache(CancellationToken cancellationToken = default)
        {
            MixFileHelper.DeleteFolder(_cacheFolder);
            return Task.CompletedTask;
        }
    }
}
