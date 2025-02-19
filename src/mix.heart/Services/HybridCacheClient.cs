using Mix.Heart.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;
using Mix.Heart.Helpers;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.IdentityModel.Tokens;
using Pomelo.EntityFrameworkCore.MySql.Storage.Internal;
namespace Mix.Heart.Services
{
    public class HybridCacheClient : IDitributedCacheClient
    {
        private readonly HybridCache _cache;
        private readonly HybridCacheEntryOptions _option;

        public HybridCacheClient(HybridCache cache, HybridCacheEntryOptions option)
        {
            _cache = cache;
            _option = option;
        }

        public async Task<T> GetFromCache<T>(string key, CancellationToken cancellationToken = default) where T : class
        {
            var cachedResponse = await _cache.GetOrCreateAsync(
                    key,
                    async cancel => await GetDataFromTheSourceAsync(key, cancel),
                    cancellationToken: cancellationToken);
            return cachedResponse.Length == 0 ? default : ReflectionHelper.FromByteArray<T>(cachedResponse);
        }

        private async Task<byte[]?> GetDataFromTheSourceAsync(string key, CancellationToken cancel)
        {
            return null;
        }

        public async Task SetCache<T>(string key, T value, TimeSpan? cacheExpiration = default, CancellationToken cancellationToken = default) where T : class
        {
            await _cache.SetAsync(key, ReflectionHelper.ToByteArray(value),
                options: cacheExpiration.HasValue ? new HybridCacheEntryOptions()
                {
                    Expiration = cacheExpiration.Value
                } : _option,
                tags: new[] { key[..key.LastIndexOf(':')], "mix" },
                cancellationToken: cancellationToken);
        }

        public async Task ClearCache(string key, CancellationToken cancellationToken = default)
        {
            await _cache.RemoveByTagAsync(key);
        }

        public async Task ClearAllCache(CancellationToken cancellationToken = default)
        {
            await _cache.RemoveByTagAsync("mix", cancellationToken);
        }
    }
}
