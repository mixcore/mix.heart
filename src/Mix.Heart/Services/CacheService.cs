using Mix.Domain.Core.ViewModels;
using Mix.Domain.Data.Repository;
using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;

namespace Mix.Services
{
    public class CacheService
    {
        /// <summary>
        /// The synchronize root
        /// </summary>
        private static readonly object syncRoot = new Object();
        /// <summary>
        /// The instance
        /// </summary>
        private static volatile CacheService instance;
        private const string cacheFolder = "cache";
        public CacheService()
        {
        }

        public static CacheService Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                        {
                            instance = new CacheService();
                        }
                    }
                }

                return instance;
            }
        }
        public static T Get<T>(string key, string folder = null)
        {
            var data = FileRepository.Instance.GetFile(key, ".json", $"{cacheFolder}/{folder}", false, "{}");
            if (data != null && !string.IsNullOrEmpty(data.Content))
            {
                var jobj = JObject.Parse(data.Content);
                return jobj.ToObject<T>();
            }
            return default(T);
        }

        public static RepositoryResponse<bool> Set<T>(string key, T value, string folder = null)
        {

            if (value != null)
            {
                var jobj = JObject.FromObject(value);
                var cacheFile = new FileViewModel()
                {
                    Filename = key.ToLower(),
                    Extension = ".json",
                    FileFolder = $"{cacheFolder}/{folder}",
                    Content = jobj.ToString(Newtonsoft.Json.Formatting.None)
                };

                var result = FileRepository.Instance.SaveFile(cacheFile);
                return new RepositoryResponse<bool>()
                {
                    IsSucceed = result
                };
            }
            else
            {
                return new RepositoryResponse<bool>();
            }
        }
        public static Task<T> GetAsync<T>(string key, string folder = null)
        {
            var data = FileRepository.Instance.GetFile(key, ".json", $"{cacheFolder}/{folder}", false, "{}");
            if (data != null && !string.IsNullOrEmpty(data.Content))
            {
                var jobj = JObject.Parse(data.Content);
                return Task.FromResult(jobj.ToObject<T>());
            }
            return Task.FromResult(default(T));
        }

        public static Task<RepositoryResponse<bool>> SetAsync<T>(string key, T value, string folder = null)
        {

            if (value != null)
            {
                var jobj = JObject.FromObject(value);
                var cacheFile = new FileViewModel()
                {
                    Filename = key.ToLower(),
                    Extension = ".json",
                    FileFolder = $"{cacheFolder}/{folder}",
                    Content = jobj.ToString(Newtonsoft.Json.Formatting.None)
                };

                var result = FileRepository.Instance.SaveFile(cacheFile);
                return Task.FromResult(new RepositoryResponse<bool>()
                {
                    IsSucceed = result
                });
            }
            else
            {
                return Task.FromResult(new RepositoryResponse<bool>());
            }            
        }

        public static Task RemoveCacheAsync()
        {
            return Task.FromResult(FileRepository.Instance.EmptyFolder(cacheFolder));
        }
        public static Task RemoveCacheAsync(string folder)
        {
            return Task.FromResult(FileRepository.Instance.DeleteFolder($"{cacheFolder}/{folder}"));
        }
    }
}
