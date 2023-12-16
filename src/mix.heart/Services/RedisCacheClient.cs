using Microsoft.Extensions.Caching.Distributed;
using Mix.Heart.Interfaces;
using Newtonsoft.Json;
using StackExchange.Redis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Mix.Heart.Services
{
    public class RedisCacheClient : IDitributedCacheClient
    {
        private readonly ConnectionMultiplexer _connectionMultiplexer;
        private readonly IDatabase _database;
        private readonly IDistributedCache _cache;
        private readonly DistributedCacheEntryOptions _options;
        public RedisCacheClient(string connectionString, IDistributedCache cache, DistributedCacheEntryOptions options)
        {
            _connectionMultiplexer = ConnectionMultiplexer.Connect(connectionString);
            _database = _connectionMultiplexer.GetDatabase();
            _cache = cache;
            _options = options;
        }

        public async Task<T> GetFromCache<T>(string key, CancellationToken cancellationToken = default) where T : class
        {
            var cachedResponse = await _cache.GetStringAsync(key, cancellationToken);
            return cachedResponse == null ? null : JsonConvert.DeserializeObject<T>(cachedResponse);
        }

        public async Task SetCache<T>(string key, T value, CancellationToken cancellationToken = default) where T : class
        {
            var response = JsonConvert.SerializeObject(value);
            await _cache.SetStringAsync(key, response, cancellationToken);
        }

        public async Task ClearCache(string key, CancellationToken cancellationToken = default)
        {
            var endpoints = _connectionMultiplexer.GetEndPoints();
            foreach (var endpoint in endpoints)
            {
                var server = _connectionMultiplexer.GetServer(endpoint);
                var keys = server.Keys().Select(m => m.ToString()).Where(k => k.Contains(key)).ToList();
                foreach (var k in keys)
                {
                    await _database.KeyDeleteAsync(k);
                }
            }
        }

        public async Task ClearAllCache(CancellationToken cancellationToken = default)
        {
            var endpoints = _connectionMultiplexer.GetEndPoints();
            foreach (var endpoint in endpoints)
            {
                var server = _connectionMultiplexer.GetServer(endpoint);
                var keys = server.Keys();
                foreach (var key in keys)
                {
                    await _database.KeyDeleteAsync(key);
                }
            }
        }
    }
}
