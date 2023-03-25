using Azure.Core.Serialization;
using Microsoft.Extensions.Caching.Distributed;
using Mix.Heart.Helpers;
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
        private readonly IDistributedCache _cache;

        public MixDitributedCache(IDistributedCache cache)
        {
            
            _cache = cache;
        }

        public async Task<T> GetFromCache<T>(string key, CancellationToken cancellationToken = default) where T : class
        {
            var cachedResponse = await _cache.GetStringAsync(key);
            return cachedResponse == null ? null : JsonConvert.DeserializeObject<T>(cachedResponse);
        }

        public async Task SetCache<T>(string key, T value, DistributedCacheEntryOptions options) where T : class
        {
            var response = JsonConvert.SerializeObject(value);
            await _cache.SetStringAsync(key, response, options);
        }

        public async Task ClearCache(string key)
        {
            await _cache.RemoveAsync(key);
        }
    }
}
