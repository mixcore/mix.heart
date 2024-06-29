using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Mix.Heart.Entities.Cache;
using Mix.Heart.Factories;
using Mix.Heart.Interfaces;
using Mix.Heart.Models;
using Mix.Heart.UnitOfWork;
using Mix.Shared.Services;
using System.Threading;
using System.Threading.Tasks;

namespace Mix.Heart.Services
{
    public class MixDitributedCache
    {
        private readonly IDitributedCacheClient? _cacheClient;
        private readonly MixHeartConfigurationModel _configs;
        public MixDitributedCache(IConfiguration configuration, IDistributedCache cache, UnitOfWorkInfo<MixCacheDbContext> cacheUow)
        {
            _configs = MixHeartConfigService.Instance.AppSettings;
            _cacheClient = CacheEngineFactory.CreateCacheClient(_configs, cacheUow, configuration, cache);
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
