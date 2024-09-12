using Microsoft.EntityFrameworkCore;
using Mix.Heart.Entities.Cache;
using Mix.Heart.Interfaces;
using Mix.Heart.Repository;
using Mix.Heart.UnitOfWork;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Mix.Heart.Services
{
    [Obsolete("This feature will be removed.", true)]
    public class MixDatabaseCacheClient : IDitributedCacheClient
    {
        private readonly JsonSerializer serializer;
        private readonly EntityRepository<MixCacheDbContext, MixCache, Guid> _repository;
        public MixDatabaseCacheClient(UnitOfWorkInfo<MixCacheDbContext> uow)
        {
            _repository = new(uow);
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
            try
            {
                var cache = await _repository.GetFirstAsync(m => m.Keyword == key, cancellationToken);
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

        public async Task SetCache<T>(string key, T value, CancellationToken cancellationToken = default) where T : class
        {
            if (value != null)
            {
                var jobj = JObject.FromObject(value);

                var cache = new MixCache()
                {
                    Id = Guid.NewGuid(),
                    Keyword = key,
                    Value = jobj.ToString(Formatting.None),
                    CreatedDateTime = DateTime.UtcNow
                };

                await _repository.SaveAsync(cache);
            }
        }

        public async Task ClearCache(string key, CancellationToken cancellationToken = default)
        {
            await _repository.DeleteAsync(m => EF.Functions.Like(m.Keyword, $"{key}%"));
        }

        public async Task ClearAllCache(CancellationToken cancellationToken = default)
        {
            await _repository.DeleteManyAsync(m => true);
        }
    }
}
