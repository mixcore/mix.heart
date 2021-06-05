using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Mix.Example.Infrastructure;

namespace Mix.Example.Infrastructure
{
    public static class InfrastructureAppBuilderExtension
    {
        public static void InitialDb(this IApplicationBuilder app)
        {
            using (var serviceScope =
                app
                .ApplicationServices
                .GetService<IServiceScopeFactory>()
                .CreateScope())
            {
                using (var context = serviceScope.ServiceProvider.GetRequiredService<MixDbContext>())
                {
                    context.Database.Migrate();
                }

                using (var context = serviceScope.ServiceProvider.GetRequiredService<ExternalDbContext>())
                {
                    context.Database.Migrate();
                }
            }
        }
    }
}
