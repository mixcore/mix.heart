using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Mix.Example.Infrastructure
{
    public static class InfrastructureAppBuilderExtension
    {
        public static void InitMixDb(this IApplicationBuilder app)
        {
            using (var serviceScope =
                app
                .ApplicationServices
                .GetService<IServiceScopeFactory>()
                .CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetRequiredService<MixDbContext>();
                context.Database.Migrate();
            }
        }
    }
}
