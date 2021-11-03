
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Mix.Heart.EntityFrameworkCore.Extensions;

namespace Mix.Heart.Entities.Cache {
  public partial class MsSqlCacheDbContext : MixCacheDbContext {
    public MsSqlCacheDbContext(IConfiguration configuration)
        : base(configuration) {}
    protected override void OnModelCreating(ModelBuilder modelBuilder) {
      modelBuilder.ApplyAllConfigurationsFromNamespace(
          this.GetType().Assembly,
          "Mix.Heart.Infrastructure.EntityConfigurations.MSSQL");
      OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
  }
}
