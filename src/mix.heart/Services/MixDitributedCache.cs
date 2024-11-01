using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Mix.Heart.Factories;
using Mix.Heart.Interfaces;
using Mix.Heart.Models;
using System.Threading;
using System.Threading.Tasks;

namespace Mix.Heart.Services
{
    public class MixDitributedCache
    {
        private readonly IDitributedCacheClient _cacheClient;
        private readonly MixHeartConfigurationModel _configs;
        public MixDitributedCache(IConfiguration configuration, IDistributedCache cache)
        {
            _configs = configuration.Get<MixHeartConfigurationModel>();
            _cacheClient = CacheEngineFactory.CreateCacheClient(_configs, configuration, cache);
        }

        public async Task<T> GetFromCache<T>(string key, CancellationToken cancellationToken = default) where T : class
        {
            if (_cacheClient != null)
            {
                var result = await _cacheClient?.GetFromCache<T>(key, cancellationToken);
                return result ?? default;
            }
            return default;
        }

        public Task SetCache<T>(string key, T value, CancellationToken cancellationToken = default) where T : class
        {
            if (_cacheClient != null)
            {
                return _cacheClient?.SetCache<T>(key, value, cancellationToken);
            }
            return Task.CompletedTask;
        }

        public Task ClearCache(string key, CancellationToken cancellationToken = default)
        {
            if (_cacheClient != null)
            {
                return _cacheClient?.ClearCache(key, cancellationToken);
            }
            return Task.CompletedTask;
        }

        public Task ClearAllCache(CancellationToken cancellationToken = default)
        {
            if (_cacheClient != null)
            {
                return _cacheClient?.ClearAllCache(cancellationToken);
            }
            return Task.CompletedTask;
        }
    }
}
