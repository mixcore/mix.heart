using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Mix.Heart.Entities.Cache;
using Mix.Heart.Enums;
using Mix.Heart.Interfaces;
using Mix.Heart.Models;
using Mix.Heart.Services;
using Mix.Heart.UnitOfWork;
using System;

namespace Mix.Heart.Factories
{
    public class CacheEngineFactory
    {
        public static IDitributedCacheClient CreateCacheClient(
            MixHeartConfigurationModel mixHeartConfiguration,
            UnitOfWorkInfo<MixCacheDbContext> uow = null,
            IConfiguration configuration = null,
            IDistributedCache cache = null)
        {
            IDitributedCacheClient cacheClient = null;
            switch (mixHeartConfiguration.CacheMode)
            {
                case MixCacheMode.JSON:
                    cacheClient = new MixStaticFileCacheClient(mixHeartConfiguration.CacheFolder);
                    break;
                case MixCacheMode.DATABASE:
                    cacheClient = new MixDatabaseCacheClient(uow);
                    break;
                case MixCacheMode.REDIS:

                    var _configs = new RedisCacheConfigurationModel();
                    configuration.GetSection("Redis").Bind(_configs);

                    var options = new DistributedCacheEntryOptions()
                    {
                        SlidingExpiration = TimeSpan.FromMinutes(_configs.SlidingExpirationInMinute),
                    };
                    if (_configs.AbsoluteExpirationInMinute.HasValue)
                    {
                        options.SetAbsoluteExpiration(TimeSpan.FromMinutes(_configs.AbsoluteExpirationInMinute.Value));
                    }
                    if (_configs.AbsoluteExpirationRelativeToNowInMinute.HasValue)
                    {
                        options.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(_configs.AbsoluteExpirationRelativeToNowInMinute.Value);
                    }

                    cacheClient = new RedisCacheClient(_configs.ConnectionString, cache, options);
                    break;
                default:
                    break;
            }
            return cacheClient;
        }
    }
}
