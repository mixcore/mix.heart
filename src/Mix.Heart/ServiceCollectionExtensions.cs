using Microsoft.Extensions.Configuration;
using Mix.Heart.Entities.Cache;
using Mix.Heart.Repository;
using Mix.Heart.Services;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMixCache(this IServiceCollection services, IConfiguration configuration)
        {

            services.AddSingleton<MixCacheDbContext>();
            services.AddSingleton<EntityRepository<MixCacheDbContext, MixCache, Guid>>();
            string redisConnection = configuration.GetSection("Redis")["ConnectionString"];
            if (!string.IsNullOrEmpty(redisConnection))
            {
                services.AddStackExchangeRedisCache(options =>
                {
                    options.Configuration = configuration.GetSection("Redis")["ConnectionString"];
                });
            }
            services.AddSingleton<MixDitributedCache>();
            services.AddSingleton<MixCacheService>();

            return services;
        }
    }
}
