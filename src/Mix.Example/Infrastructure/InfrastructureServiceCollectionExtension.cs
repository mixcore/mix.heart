using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Mix.Example.Infrastructure;

namespace Mix.Example.Infrastructure
{
    public static class InfrastructureServiceCollectionExtension
    {
        public static void AddMixDbContext(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionStrings = configuration.GetConnectionString("MixDb");
            services.AddDbContext<MixDbContext>();
        }

        public static void AddExternalDbContext(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionStrings = configuration.GetConnectionString("ExternalDb");
            services.AddDbContext<ExternalDbContext>();
        }
    }
}
