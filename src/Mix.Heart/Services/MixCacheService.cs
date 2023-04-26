using Mix.Heart.Entities.Cache;
using Mix.Heart.Models;
using Mix.Heart.Repository;
using Mix.Shared.Services;
using Newtonsoft.Json;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Mix.Heart.Services
{
    public class MixCacheService
    {
        private readonly MixDitributedCache _cache;
        private readonly MixHeartConfigurationModel _configs;
        private readonly EntityRepository<MixCacheDbContext, MixCache, Guid> _repository;
        public bool IsCacheEnabled { get => _configs.IsCache; }
        protected JsonSerializer serializer;

        public MixCacheService(MixDitributedCache ditributedCache)
        {
            _configs = MixHeartConfigService.Instance.AppSettings;
            _cache = ditributedCache;
        }


        #region Get

        public Task<T> GetAsync<T>(string key, string cacheFolder, string filename, CancellationToken cancellationToken = default)
            where T : class
        {
            return _cache.GetFromCache<T>($"{cacheFolder}_{key}_{filename}", cancellationToken);
        }
        #endregion

        #region Set

        public Task SetAsync<T>(string key, T value, string cacheFolder, string filename, CancellationToken cancellationToken = default)
            where T : class
        {
            return _cache.SetCache($"{cacheFolder}_{key}_{filename}", value, cancellationToken);
        }
        #endregion

        #region Clear

        public Task ClearAllCacheAsync(CancellationToken cancellationToken = default)
        {
            return _cache.ClearAllCache(cancellationToken);
        }

        public Task RemoveCacheAsync(object key, string cacheFolder, CancellationToken cancellationToken = default)
        {
            return _cache.ClearCache($"{cacheFolder}_{key}", cancellationToken);
        }
        #endregion
    }
}