using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Mix.Heart.Entities.Cache;
using Mix.Heart.Repository;
using Mix.Heart.Services;
using Mix.Heart.UnitOfWork;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMixCache(this IServiceCollection services, IConfiguration configuration)
        {

            services.TryAddScoped<MixCacheDbContext>();
            services.TryAddScoped<UnitOfWorkInfo<MixCacheDbContext>>();
            services.TryAddScoped<EntityRepository<MixCacheDbContext, MixCache, Guid>>();
            string redisConnection = configuration.GetSection("Redis")["ConnectionString"];
            if (!string.IsNullOrEmpty(redisConnection))
            {
                services.AddStackExchangeRedisCache(options =>
                {
                    options.Configuration = configuration.GetSection("Redis")["ConnectionString"];
                });
            }
            else
            {
                services.AddDistributedMemoryCache();
            }
            services.TryAddScoped<MixDitributedCache>();
            services.TryAddScoped<MixCacheService>();


            return services;
        }
    }
}
