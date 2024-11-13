using Microsoft.Extensions.Caching.Distributed;
using Mix.Heart.Extensions;
using Mix.Heart.Interfaces;
using StackExchange.Redis;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Buffers.Text;
using System.Text;
using Newtonsoft.Json;
namespace Mix.Heart.Services
{
    public class RedisCacheClient : IDitributedCacheClient
    {
        private IDatabase _database;
        private readonly IDistributedCache _cache;
        private readonly DistributedCacheEntryOptions _options;
        private ConnectionMultiplexer _connectionMultiplexer;
        private readonly string _connectionString;
        public RedisCacheClient(string connectionString, IDistributedCache cache, DistributedCacheEntryOptions options)
        {
            _cache = cache;
            _options = options;
            _connectionString = connectionString;
            
        }

        public async Task<T> GetFromCache<T>(string key, CancellationToken cancellationToken = default) where T : class
        {
            await TryConnect();
            var cachedResponse = await _cache.GetAsync(key, cancellationToken);
            return cachedResponse == null ? null : JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(cachedResponse));
        }

        
        public async Task SetCache<T>(string key, T value, TimeSpan? cacheExpiration = default, CancellationToken cancellationToken = default) where T : class
        {
            await TryConnect();
            if (cacheExpiration != null)
            {
                _options.SlidingExpiration = cacheExpiration;
            }
            await _cache.SetAsync(key, JsonConvert.SerializeObject(value).ToByteArray(), _options, cancellationToken);
        }

        public async Task ClearCache(string key, CancellationToken cancellationToken = default)
        {
            await TryConnect();
            var endpoints = _connectionMultiplexer.GetEndPoints();
            foreach (var endpoint in endpoints)
            {
                var server = _connectionMultiplexer.GetServer(endpoint);
                await _database.KeyDeleteAsync(server.Keys(pattern: $"{key}:*").ToArray());
            }
        }

        public async Task ClearAllCache(CancellationToken cancellationToken = default)
        {
            await TryConnect();
            var endpoints = _connectionMultiplexer.GetEndPoints();
            try
            {
                foreach (var endpoint in endpoints)
                {
                    var server = _connectionMultiplexer.GetServer(endpoint);
                    await server.FlushDatabaseAsync();
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
            }
        }

        #region Binary Helper


        #endregion

        private async Task TryConnect()
        {
            while (_connectionMultiplexer == null || !_connectionMultiplexer.IsConnected)
            {
                _connectionMultiplexer = await ConnectionMultiplexer.ConnectAsync(_connectionString, m => m.AllowAdmin = true);
                _database = _connectionMultiplexer.GetDatabase();
                if (!_connectionMultiplexer.IsConnected)
                {
                    Thread.Sleep(2000);
                }
            }
        }

    }
}
