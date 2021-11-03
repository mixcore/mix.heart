
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Mix.Heart.EntityFrameworkCore.Extensions;

namespace Mix.Heart.Entities.Cache {
  public partial class PostgresCacheDbContext : MixCacheDbContext {
    public PostgresCacheDbContext(IConfiguration configuration)
        : base(configuration) {}
    protected override void OnModelCreating(ModelBuilder modelBuilder) {
      modelBuilder.ApplyAllConfigurationsFromNamespace(
          this.GetType().Assembly,
          "Mix.Heart.Infrastructure.EntityConfigurations.POSTGRES");
      OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
  }
}
