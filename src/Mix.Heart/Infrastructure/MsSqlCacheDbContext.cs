
using Microsoft.EntityFrameworkCore;
using Mix.Heart.Extensions;

namespace Mix.Heart.Infrastructure
{
    public partial class MsSqlCacheDbContext: MixCacheDbContext
    {
        public MsSqlCacheDbContext()
        {

        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyAllConfigurationsFromNamespace(
                this.GetType().Assembly,
                "Mix.Heart.Infrastructure.EntityConfigurations.MSSQL");
            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
