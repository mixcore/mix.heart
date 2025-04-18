﻿using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Mix.Heart.Enums;
using Mix.Heart.Interfaces;
using Mix.Heart.Models;
using Mix.Heart.Services;
using System;

namespace Mix.Heart.Factories
{
    public class CacheEngineFactory
    {
        public static IDitributedCacheClient CreateCacheClient(
            MixHeartConfigurationModel mixHeartConfiguration,
            HybridCache hybridCache,
            IConfiguration configuration = null,
            IDistributedCache cache = null)
        {
            IDitributedCacheClient cacheClient = null;
            switch (mixHeartConfiguration.CacheMode)
            {
                case MixCacheMode.JSON:
                    cacheClient = new MixStaticFileCacheClient(mixHeartConfiguration.CacheFolder);
                    break;
                case MixCacheMode.HYBRID:
                    cacheClient = new HybridCacheClient(hybridCache, new HybridCacheEntryOptions()
                    {
                        Expiration = TimeSpan.FromMinutes(mixHeartConfiguration.SlidingExpirationInMinute),
                        LocalCacheExpiration = TimeSpan.FromMinutes(mixHeartConfiguration.SlidingExpirationInMinute),
                    });
                    break;
                case MixCacheMode.REDIS:
                    try
                    {
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
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Cannot create redis client: {ex.Message}, using JSON cache instead");
                        return default;
                    }
                    break;
                default:
                    break;
            }
            return cacheClient;
        }
    }
}
