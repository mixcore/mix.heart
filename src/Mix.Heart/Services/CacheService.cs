using Mix.Domain.Core.ViewModels;
using Mix.Domain.Data.Repository;
using Mix.Heart.Helpers;
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
        private const string ext = ".json";
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
            try
            {
                var cachedFile = FileRepository.Instance.GetFile(key, ext, $"{cacheFolder}/{folder}", false, "");
                if (!string.IsNullOrEmpty(cachedFile.Content))
                {
                    //return GetFromBase64<T>(cachedFile);
                    return GetFromJson<T>(cachedFile);
                }
                else
                {
                    return default(T);
                }
            }
            catch (Exception ex)
            {
                //TODO Handle Exception
                Console.WriteLine(ex);
                return default(T);
            }
        }

        private static T GetFromJson<T>(FileViewModel cachedFile)
        {
            var jobj = JObject.Parse(cachedFile.Content);
            return jobj.ToObject<T>();
        }

        private static T GetFromBase64<T>(FileViewModel cachedFile)
        {
            //var encryptKey = System.Text.Encoding.UTF8.GetBytes("sw-cms-secret-key");
            if (!string.IsNullOrEmpty(cachedFile.Content))
            {
                var data = Convert.FromBase64String(cachedFile.Content);
                if (data != null)
                {
                    var jobj = JObject.Parse(System.Text.Encoding.UTF8.GetString(data));
                    return jobj.ToObject<T>();
                }
            }
            return default(T);
        }

        public static RepositoryResponse<bool> Set<T>(string key, T value, string folder = null)
        {

            if (value != null)
            {
                //var result = SaveBase64(key, value, folder);
                var result = SaveJson(key, value, folder);
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
        private static bool SaveJson<T>(string key, T value, string folder)
        {
            var jobj = JObject.FromObject(value);

            var cacheFile = new FileViewModel()
            {
                Filename = key.ToLower(),
                Extension = ext,
                FileFolder = $"{cacheFolder}/{folder}",
                Content = jobj.ToString(Newtonsoft.Json.Formatting.None)
            };
            return FileRepository.Instance.SaveFile(cacheFile);
        }
        private static bool SaveBase64<T>(string key, T value, string folder)
        {
            var jobj = JObject.FromObject(value);
            var data = System.Text.Encoding.UTF8.GetBytes(jobj.ToString(Newtonsoft.Json.Formatting.None));

            var cacheFile = new FileViewModel()
            {
                Filename = key.ToLower(),
                Extension = ext,
                FileFolder = $"{cacheFolder}/{folder}",
                Content = Convert.ToBase64String(data)
            };


            return FileRepository.Instance.SaveFile(cacheFile);
        }

        public static Task<T> GetAsync<T>(string key, string folder = null)
        {
            try
            {
                var cachedFile = FileRepository.Instance.GetFile(key, ext, $"{cacheFolder}/{folder}", false, "");
                if (!string.IsNullOrEmpty(cachedFile.Content))
                {
                    //return GetFromBase64<T>(cachedFile);
                    return Task.FromResult(GetFromJson<T>(cachedFile));
                }
                else
                {
                    return Task.FromResult(default(T));
                }
            }
            catch(Exception ex)
            {
                //TODO Handle Exception
                Console.WriteLine(ex);
                return Task.FromResult(default(T));
            }
            
        }

        public static Task<RepositoryResponse<bool>> SetAsync<T>(string key, T value, string folder = null)
        {

            if (value != null)
            {
                //var result = SaveBase64(key, value, folder);
                var result = SaveJson(key, value, folder);
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
