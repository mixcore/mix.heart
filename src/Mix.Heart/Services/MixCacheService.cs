using Microsoft.EntityFrameworkCore;
using Mix.Common.Helper;
using Mix.Heart.Constants;
using Mix.Heart.Enums;
using Mix.Heart.Infrastructure.ViewModels;
using Mix.Heart.Models;
using Mix.Infrastructure.Repositories;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace Mix.Services
{
    public class MixCacheService
    {
        private static string cacheFolder = CommonHelper.GetWebConfig<string>(WebConfiguration.MixCacheFolder);

        static MixCacheService()
        {
            using (var ctx = new MixCacheDbContext())
            {
                ctx.Database.Migrate();
            }
        }

        public static T Get<T>(string key, string folder = null)
        {
            try
            {
                var cacheMode = CommonHelper.GetWebEnumConfig<MixCacheMode>(WebConfiguration.MixCacheMode);
                switch (cacheMode)
                {
                    case MixCacheMode.Database:
                        return GetFromDatabase<T>(key, folder);
                    case MixCacheMode.Binary:
                        return GetFromBinary<T>(key, folder);
                    case MixCacheMode.Base64:
                        return GetFromBase64<T>(key, folder);
                    case MixCacheMode.Json:
                    default:
                        return GetFromJson<T>(key, folder);
                }
            }
            catch (Exception ex)
            {
                //TODO Handle Exception
                Console.WriteLine(ex);
                return default(T);
            }
        }

        private static T GetFromDatabase<T>(string key, string folder)
        {
            try
            {
                using (var ctx = new MixCacheDbContext())
                {
                    var cache = ctx.MixCache.FirstOrDefault(m => m.Id == $"{folder}/{key}");
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
            }
            catch (Exception ex)
            {
                //TODO Handle Exception
                Console.WriteLine(ex);
                return default;
            }
        }

        private static T GetFromJson<T>(string key, string folder = null)
        {
            string filePath = $"{cacheFolder}/{folder}/{key}.json";
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


        private static T GetFromBase64<T>(string key, string folder)
        {
            var cachedFile = MixFileRepository.Instance.GetFile(key, string.Empty, $"{cacheFolder}/{folder}", false, string.Empty);
            if (!string.IsNullOrEmpty(cachedFile.Content))
            {
                var data = Convert.FromBase64String(cachedFile.Content);
                if (data != null)
                {
                    var jobj = JObject.Parse(Encoding.Unicode.GetString(data));
                    return jobj.ToObject<T>();
                }
            }
            return default(T);
        }

        private static T GetFromBinary<T>(string key, string folder)
        {
            var cachedFile = MixFileRepository.Instance.GetFile(key, ".bin", $"{ cacheFolder}/{folder}", false, string.Empty);
            byte[] data = Encoding.Unicode.GetBytes(cachedFile.Content);
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
                var result = new RepositoryResponse<bool>();
                var cacheMode = CommonHelper.GetWebEnumConfig<MixCacheMode>(WebConfiguration.MixCacheMode);
                switch (cacheMode)
                {
                    case MixCacheMode.Database:
                        result.IsSucceed = SaveDatabase(key, value, folder);
                        break;
                    case MixCacheMode.Binary:
                        result.IsSucceed = SaveBinary(key, value, folder);
                        break;
                    case MixCacheMode.Base64:
                        result.IsSucceed = SaveBase64(key, value, folder);
                        break;
                    case MixCacheMode.Json:
                    default:
                        result.IsSucceed = SaveJson(key, value, folder);
                        break;
                }
                return result;
            }
            else
            {
                return new RepositoryResponse<bool>();
            }
        }

        private static bool SaveDatabase<T>(string key, T value, string folder)
        {
            var result = new RepositoryResponse<MixCacheViewModel>();
            if (value != null)
            {
                var jobj = JObject.FromObject(value);

                var cache = new MixCacheViewModel()
                {
                    Id = $"{folder}/{key}",
                    Value = jobj.ToString(Newtonsoft.Json.Formatting.None),
                    CreatedDateTime = DateTime.UtcNow
                };

                result = cache.SaveModel();
            }
            return result.IsSucceed;
        }

        private static bool SaveJson<T>(string key, T value, string folder)
        {
            try
            {
                var jobj = JObject.FromObject(value);

                var cacheFile = new FileViewModel()
                {
                    Filename = key.ToLower(),
                    Extension = ".json",
                    FileFolder = $"{cacheFolder}/{folder}",
                    Content = jobj.ToString(Newtonsoft.Json.Formatting.None)
                };
                return MixFileRepository.Instance.SaveFile(cacheFile);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return false;
            }
        }

        private static bool SaveBase64<T>(string key, T value, string folder)
        {
            var jobj = JObject.FromObject(value);
            var data = Encoding.Unicode.GetBytes(jobj.ToString(Formatting.None));

            var cacheFile = new FileViewModel()
            {
                Filename = key.ToLower(),
                Extension = string.Empty,
                FileFolder = $"{cacheFolder}/{folder}",
                Content = Convert.ToBase64String(data)
            };
            return MixFileRepository.Instance.SaveFile(cacheFile);
        }

        private static bool SaveBinary<T>(string key, T value, string folder)
        {
            string saveFolder = $"{cacheFolder}/{folder}/";
            MixFileRepository.Instance.CreateDirectoryIfNotExist(saveFolder);
            try
            {
                string filename = $"{saveFolder}/{key.ToLower()}.bin";
                //Create the stream to add object into it.  
                System.IO.Stream ms = File.OpenWrite(filename);

                //Format the object as Binary  
                BinaryFormatter formatter = new BinaryFormatter();

                //It serialize the employee object  
                formatter.Serialize(ms, value);
                ms.Flush();
                ms.Close();
                ms.Dispose();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return false;
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
                var cacheMode = CommonHelper.GetWebEnumConfig<MixCacheMode>(WebConfiguration.MixCacheMode);
                switch (cacheMode)
                {
                    case MixCacheMode.Database:
                        return GetFromDatabaseAsync<T>(key, folder);
                    case MixCacheMode.Binary:
                        return Task.FromResult(GetFromBinary<T>(key, folder));
                    case MixCacheMode.Base64:
                        return Task.FromResult(GetFromBase64<T>(key, folder));
                    case MixCacheMode.Json:
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

        private static async Task<T> GetFromDatabaseAsync<T>(string key, string folder)
        {
            try
            {
                using (var ctx = new MixCacheDbContext())
                {
                    var cache = await ctx.MixCache.FirstOrDefaultAsync(m => m.Id == $"{folder}/{key}");
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
            }
            catch (Exception ex)
            {
                //TODO Handle Exception
                Console.WriteLine(ex);
                return default;
            }
        }

        public static async Task<RepositoryResponse<bool>> SetAsync<T>(string key, T value, string folder = null)
        {
            if (value != null)
            {
                var result = new RepositoryResponse<bool>();
                var cacheMode = CommonHelper.GetWebEnumConfig<MixCacheMode>(WebConfiguration.MixCacheMode);
                switch (cacheMode)
                {
                    case MixCacheMode.Database:
                        result.IsSucceed = await SaveDatabaseAsync(key, value, folder);
                        break;
                    case MixCacheMode.Binary:
                        result.IsSucceed = SaveBinary(key, value, folder);
                        break;
                    case MixCacheMode.Base64:
                        result.IsSucceed = SaveBase64(key, value, folder);
                        break;
                    case MixCacheMode.Json:
                    default:
                        result.IsSucceed = SaveJson(key, value, folder);
                        break;
                }
                return result;
            }
            else
            {
                return new RepositoryResponse<bool>();
            }
        }

        private static async Task<bool> SaveDatabaseAsync<T>(string key, T value, string folder)
        {
            var result = new RepositoryResponse<MixCacheViewModel>();
            if (value != null)
            {
                var jobj = JObject.FromObject(value);

                var cache = new MixCacheViewModel()
                {
                    Id = $"{folder}/{key}",
                    Value = jobj.ToString(Formatting.None),
                    CreatedDateTime = DateTime.UtcNow
                };

                result = await cache.SaveModelAsync();
            }
            return result.IsSucceed;
        }

        public static Task RemoveCacheAsync()
        {
            var cacheMode = CommonHelper.GetWebEnumConfig<MixCacheMode>(WebConfiguration.MixCacheMode);
            switch (cacheMode)
            {
                case MixCacheMode.Database:
                    using (var ctx = new MixCacheDbContext())
                    {
                        ctx.MixCache.RemoveRange(ctx.MixCache);
                        ctx.SaveChangesAsync();
                        return Task.CompletedTask;
                    }
                case MixCacheMode.Json:
                case MixCacheMode.Binary:
                case MixCacheMode.Base64:
                case MixCacheMode.Memory:
                default:
                    return Task.FromResult(MixFileRepository.Instance.EmptyFolder(cacheFolder));
            }
        }

        public static Task RemoveCacheAsync(string folder)
        {
            var cacheMode = CommonHelper.GetWebEnumConfig<MixCacheMode>(WebConfiguration.MixCacheMode);
            switch (cacheMode)
            {
                case MixCacheMode.Database:
                    using (var ctx = new MixCacheDbContext())
                    {
                        ctx.MixCache.RemoveRange(ctx.MixCache.Where(m => EF.Functions.Like(m.Id, $"%{folder}%")));
                        ctx.SaveChangesAsync();
                        return Task.CompletedTask;
                    }
                case MixCacheMode.Json:
                case MixCacheMode.Binary:
                case MixCacheMode.Base64:
                case MixCacheMode.Memory:
                default:
                    return Task.FromResult(MixFileRepository.Instance.DeleteFolder(
                        $"{cacheFolder}/{folder}"));
            }
        }

        public static Task RemoveCacheAsync(Type type, string key = null)
        {
            string path = $"{cacheFolder}/{type.FullName}/";
            if (!string.IsNullOrEmpty(key))
            {
                path += $"/{key}";
            }
            return Task.FromResult(MixFileRepository.Instance.EmptyFolder(path));
        }
    }
}