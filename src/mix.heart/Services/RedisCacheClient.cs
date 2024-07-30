﻿using Microsoft.Extensions.Caching.Distributed;
using Mix.Heart.Interfaces;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Mix.Heart.Services
{
public class RedisCacheClient : IDitributedCacheClient
{
    private readonly IDatabase _database;
    private readonly IDistributedCache _cache;
    private readonly DistributedCacheEntryOptions _options;
    private readonly ConnectionMultiplexer _connectionMultiplexer;

    public RedisCacheClient(string connectionString, IDistributedCache cache, DistributedCacheEntryOptions options)
    {
        _cache = cache;
        _options = options;
        _connectionMultiplexer = ConnectionMultiplexer.Connect(connectionString, m => m.AllowAdmin = true);
        _database = _connectionMultiplexer.GetDatabase();
    }

    public async Task<T> GetFromCache<T>(string key, CancellationToken cancellationToken = default) where T : class
    {
        var cachedResponse = await _cache.GetStringAsync(key, cancellationToken);
        return cachedResponse == null ? null : JsonConvert.DeserializeObject<T>(cachedResponse);
    }

    public async Task SetCache<T>(string key, T value, CancellationToken cancellationToken = default) where T : class
    {
        var response = JsonConvert.SerializeObject(value);
        await _cache.SetStringAsync(key, response, _options, cancellationToken);
    }

    public async Task ClearCache(string key, CancellationToken cancellationToken = default)
    {
        var endpoints = _connectionMultiplexer.GetEndPoints();
        foreach (var endpoint in endpoints)
        {
            var server = _connectionMultiplexer.GetServer(endpoint);
            var keys = server.Keys(pattern: key + "*").ToList();
            foreach (var k in keys)
            {
                await _database.KeyDeleteAsync(k);
            }
        }
    }

    public async Task ClearAllCache(CancellationToken cancellationToken = default)
    {
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
}
}
