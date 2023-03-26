using Azure.Core.Serialization;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Mix.Heart.Entities.Cache;
using Mix.Heart.Factories;
using Mix.Heart.Helpers;
using Mix.Heart.Interfaces;
using Mix.Heart.Models;
using Mix.Heart.UnitOfWork;
using Mix.Shared.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Mix.Heart.Services
{
    public class MixDitributedCache
    {
        private readonly IDitributedCacheClient _cacheClient;
        private readonly MixHeartConfigurationModel _configs;
        private readonly DistributedCacheEntryOptions _options;
        public MixDitributedCache(IConfiguration configuration, IDistributedCache cache, UnitOfWorkInfo<MixCacheDbContext> cacheUow)
        {
            _configs = MixHeartConfigService.Instance.AppSettings;
            _cacheClient = CacheEngineFactory.CreateCacheClient(_configs, cacheUow, configuration, cache);
        }

        public Task<T> GetFromCache<T>(string key, CancellationToken cancellationToken = default) where T : class
        {
            return _cacheClient.GetFromCache<T>(key, cancellationToken);
        }

        public Task SetCache<T>(string key, T value, CancellationToken cancellationToken = default) where T : class
        {
            return _cacheClient.SetCache<T>(key, value, cancellationToken);
        }

        public Task ClearCache(string key, CancellationToken cancellationToken = default)
        {
            return _cacheClient.ClearCache(key, cancellationToken);
        }

        public Task ClearAllCache(CancellationToken cancellationToken = default)
        {
            return _cacheClient.ClearAllCache(cancellationToken);
        }
    }
}
