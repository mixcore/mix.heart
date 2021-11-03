using Microsoft.Extensions.DependencyInjection;
using Mix.Heart.Entities;
using Mix.Heart.Repository;
using System;
using System.Collections.Generic;

namespace Mix.Heart.Extensions {
  public static class ServiceExtension {
    public static IServiceCollection
    AddRepositories(this IServiceCollection services,
                    IEnumerable<Type> entities, Type dbContextType) {
      var queryRepo = typeof(QueryRepository<,, >);
      var commandRepo = typeof(EntityRepository<,, >);
      foreach (var candidate in entities) {
        Type keyType = candidate.IsSubclassOf(typeof(EntityBase<int>))
                           ? typeof(int)
                           : typeof(Guid);
        Type[] types =
            new[] { dbContextType, candidate.UnderlyingSystemType, keyType };
        services.AddScoped(queryRepo.MakeGenericType(types));

        services.AddScoped(commandRepo.MakeGenericType(types));
      }
      return services;
    }
  }
}
