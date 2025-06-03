using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
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
#pragma warning disable EXTEXP0018 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
            services.AddHybridCache(options =>
            {
                options.MaximumPayloadBytes = 1024 * 1024;
                options.MaximumKeyLength = 1024;
                options.DefaultEntryOptions = new HybridCacheEntryOptions
                {
                    Expiration = TimeSpan.FromMinutes(5),
                    LocalCacheExpiration = TimeSpan.FromMinutes(5)
                };
            });
#pragma warning restore EXTEXP0018 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

            services.TryAddScoped<MixCacheDbContext>();
            services.TryAddScoped<UnitOfWorkInfo<MixCacheDbContext>>();
            services.TryAddScoped<EntityRepository<MixCacheDbContext, MixCache, Guid>>();
            string redisConnection = configuration.GetSection("Redis")["ConnectionStrings"];
            if (!string.IsNullOrEmpty(redisConnection))
            {
                services.AddStackExchangeRedisCache(options =>
                {
                    options.Configuration = configuration.GetSection("Redis")["ConnectionStrings"];
                });
            }
            else
            {
                services.AddDistributedMemoryCache();
            }
            services.TryAddSingleton<MixCacheService>();


            return services;
        }
    }
}
