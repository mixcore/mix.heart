using System.Threading;
using System.Threading.Tasks;

namespace Mix.Heart.Interfaces
{
    public interface IDitributedCacheClient
    {
        Task ClearCache(string key, CancellationToken cancellationToken = default);
        Task ClearAllCache(CancellationToken cancellationToken = default);
        Task<T> GetFromCache<T>(string key, CancellationToken cancellationToken = default) where T : class;
        Task SetCache<T>(string key, T value, CancellationToken cancellationToken = default) where T : class;
    }
}