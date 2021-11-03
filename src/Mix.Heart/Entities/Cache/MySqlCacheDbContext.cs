
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Mix.Heart.EntityFrameworkCore.Extensions;

namespace Mix.Heart.Entities.Cache
{
public partial class MySqlCacheDbContext : MixCacheDbContext
{
    public MySqlCacheDbContext(IConfiguration configuration) : base(configuration)
    {

    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyAllConfigurationsFromNamespace(
            this.GetType().Assembly,
            "Mix.Heart.Infrastructure.EntityConfigurations.MySQL");
        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
}
