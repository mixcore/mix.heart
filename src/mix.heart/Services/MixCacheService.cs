using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Mix.Heart.Factories;
using Mix.Heart.Interfaces;
using Mix.Heart.Model;
using Mix.Heart.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Mix.Heart.Services
{
    public class MixCacheService
    {
        private readonly MixDitributedCache _cache;
        private readonly MixHeartConfigurationModel _configs;
        public bool IsCacheEnabled { get => _configs.IsCache; }
        public MixCacheService(IConfiguration configuration, MixDitributedCache ditributedCache)
        {
            _configs = configuration.Get<MixHeartConfigurationModel>();
            _cache = ditributedCache;
        }

        #region Get

        public async Task<T> GetAsync<T>(string key, string cacheFolder, string filename, CancellationToken cancellationToken = default)
            where T : class
        {
            var result = await _cache.GetFromCache<T>($"{cacheFolder}:{key}_{filename}".ToLower(), cancellationToken);
            return result ?? default;
        }

        public async Task<T> GetAsync<T>(string key, CancellationToken cancellationToken = default)
            where T : class
        {
            var result = await _cache.GetFromCache<T>(key, cancellationToken);
            return result ?? default;
        }
        #endregion

        #region Set

        public Task SetAsync<T>(string key, T value, string cacheFolder, string filename, TimeSpan? cacheExpiration = null, CancellationToken cancellationToken = default)
            where T : class
        {
            return _cache.SetCache($"{cacheFolder}:{key}_{filename}".ToLower(), value, cacheExpiration, cancellationToken);
        }

        public Task SetAsync<T>(string key, T value, TimeSpan? cacheExpiration = null, CancellationToken cancellationToken = default)
            where T : class
        {
            return _cache.SetCache(key, value, cacheExpiration, cancellationToken);
        }
        #endregion

        #region Clear

        public Task ClearAllCacheAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                return _cache.ClearAllCache(cancellationToken);
            }
            catch
            {
                throw;
            }
        }

        public Task RemoveCacheAsync(object key, string cacheFolder, CancellationToken cancellationToken = default)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                return _cache.ClearCache($"{cacheFolder}:{key}".ToLower(), cancellationToken);
            }
            catch
            {
                throw;
            }
        }

        public Task RemoveCacheAsync(string key, CancellationToken cancellationToken = default)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                return _cache.ClearCache(key, cancellationToken);
            }
            catch
            {
                throw;
            }
        }


        public async Task RemoveCachesAsync(List<ModifiedEntityModel> modifiedEntities, CancellationToken cancellationToken = default)
        {
            try
            {
                foreach (var item in modifiedEntities)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    await RemoveCacheAsync(item.Id, item.CacheFolder, cancellationToken);
                }
            }
            catch
            {
                throw;
            }
        }
        #endregion
    }
}