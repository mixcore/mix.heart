using Microsoft.Extensions.DependencyInjection;
using Mix.Heart.Entities;
using Mix.Heart.Repository;
using System;
using System.Linq;
using System.Reflection;

namespace Mix.Heart.Extensions
{
    public static class ServiceExtension
    {
        public static IServiceCollection AddRepositories(this IServiceCollection services, Assembly assembly, Type dbContextType)
        {
            var candidates = assembly
                .GetExportedTypes()
                .Where(
                myType => myType.IsClass && !myType.IsAbstract && myType.BaseType.IsAbstract
                && (
                    myType.IsSubclassOf(typeof(Entity))
                    || myType.IsSubclassOf(typeof(EntityBase<int>))
                    || myType.IsSubclassOf(typeof(EntityBase<Guid>)
                )));
            var queryRepo = typeof(QueryRepository<,,>);
            var commandRepo = typeof(CommandRepository<,,>);
            foreach (var candidate in candidates)
            {
                Type keyType = candidate.IsSubclassOf(typeof(EntityBase<int>)) ? typeof(int) : typeof(Guid);
                Type[] types = new[] { dbContextType, candidate.UnderlyingSystemType, keyType };
                services.AddScoped(
                    queryRepo.MakeGenericType(types)
                );

                services.AddScoped(
                    commandRepo.MakeGenericType(types)
                );
            }
            return services;
        }
    }
}
