using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Mix.Example.Infrastructure
{
    public static class InfrastructureServiceCollectionExtension
    {
        public static void AddMixDbContext(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<MixDbContext>();
        }
    }
}
