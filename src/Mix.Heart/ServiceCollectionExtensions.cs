using Mix.Heart.Entities.Cache;
using Mix.Heart.Repository;
using Mix.Heart.Services;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMixCache(this IServiceCollection services)
        {

            services.AddSingleton<MixCacheDbContext>();
            services.AddSingleton<EntityRepository<MixCacheDbContext, MixCache, string>>();
            services.AddSingleton<MixCacheService>();

            return services;
        }
    }
}
