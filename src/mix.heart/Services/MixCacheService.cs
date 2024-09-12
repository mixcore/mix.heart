using Mix.Heart.Model;
using Mix.Heart.Models;
using Mix.Shared.Services;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Mix.Heart.Services
{
public class MixCacheService
{
    private readonly MixDitributedCache _cache;
    private readonly MixHeartConfigurationModel _configs;
    public bool IsCacheEnabled {
        get => _configs.IsCache;
    }

    public MixCacheService(MixDitributedCache ditributedCache)
    {
        _configs = MixHeartConfigService.Instance.AppSettings;
        _cache = ditributedCache;
    }

    #region Get

    public async Task<T> GetAsync<T>(string key, string cacheFolder, string filename, CancellationToken cancellationToken = default)
    where T : class
    {
        var result = await _cache.GetFromCache<T>($"{cacheFolder}_{key}_{filename}".ToLower(), cancellationToken); ;
        return result ?? default;
    }
    #endregion

    #region Set

    public Task SetAsync<T>(string key, T value, string cacheFolder, string filename, CancellationToken cancellationToken = default)
    where T : class
    {
        return _cache.SetCache($"{cacheFolder}_{key}_{filename}".ToLower(), value, cancellationToken);
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
            return _cache.ClearCache($"{cacheFolder}_{key}".ToLower(), cancellationToken);
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