using Mix.Common.Helper;
using Mix.Domain.Core.ViewModels;
using Mix.Domain.Data.Repository;
using Mix.Heart.Helpers;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using static Mix.Heart.Domain.Constants.Common;

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
        private static string cacheFolder = CommonHelper.GetWebConfig<string>(WebConfiguration.MixCacheFolder);
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
                var cachedFile = MixFileRepository.Instance.GetFile(key, ext, $"{cacheFolder}/{folder}", false, "");
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

        private static T GetFromBinary<T>(FileViewModel cachedFile)
        {
            byte[] data = Encoding.ASCII.GetBytes(cachedFile.Content);
            return ByteArrayToObject<T>(data);
        }
        // Convert a byte array to an Object
        private static T ByteArrayToObject<T>(byte[] arrBytes)
        {
            MemoryStream memStream = new MemoryStream();
            BinaryFormatter binForm = new BinaryFormatter();
            memStream.Write(arrBytes, 0, arrBytes.Length);
            memStream.Seek(0, SeekOrigin.Begin);
            T obj = (T)binForm.Deserialize(memStream);

            return obj;
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
            return MixFileRepository.Instance.SaveFile(cacheFile);
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
            return MixFileRepository.Instance.SaveFile(cacheFile);
        }

        private static bool SaveByte<T>(string key, T value, string folder)
        {
            string saveFolder = $"{cacheFolder}/{folder}/";
            MixFileRepository.Instance.CreateDirectoryIfNotExist(saveFolder);

            using (BinaryWriter binWriter =
                    new BinaryWriter(File.Open($"{saveFolder}/{key.ToLower()}.txt", FileMode.OpenOrCreate)))
            {
                binWriter.Write(ObjectToByteArray(value));
                return true;
            }
        }

        private static byte[] ObjectToByteArray(object obj)
        {
            if (obj == null)
                return null;
            BinaryFormatter bf = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream())
            {
                bf.Serialize(ms, obj);
                return ms.ToArray();
            }
        }

        public static Task<T> GetAsync<T>(string key, string folder = null)
        {
            try
            {
                var cachedFile = MixFileRepository.Instance.GetFile(key, ".txt", $"{cacheFolder}/{folder}", false, "");
                if (!string.IsNullOrEmpty(cachedFile.Content))
                {
                    //return GetFromBase64<T>(cachedFile);
                    return Task.FromResult(GetFromJson<T>(cachedFile));
                    //return Task.FromResult(GetFromBinary<T>(cachedFile));
                }
                else
                {
                    return Task.FromResult(default(T));
                }
            }
            catch (Exception ex)
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
                //var result = SaveByte(key, value, folder);
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
            return Task.FromResult(MixFileRepository.Instance.EmptyFolder(cacheFolder));
        }
        public static Task RemoveCacheAsync(string folder)
        {
            return Task.FromResult(MixFileRepository.Instance.DeleteFolder($"{cacheFolder}/{folder}"));
        }
        public static Task RemoveCacheAsync(Type type, string key = null)
        {
            string path = $"{cacheFolder}/{type.FullName.Substring(0, type.FullName.LastIndexOf('.')).Replace(".", "/")}";
            if (!string.IsNullOrEmpty(key))
            {
                path += $"/{key}";
            }
            return Task.FromResult(MixFileRepository.Instance.EmptyFolder(path));
        }
    }
}
