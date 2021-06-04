
using Microsoft.EntityFrameworkCore;
using Mix.Heart.Extensions;

namespace Mix.Heart.Infrastructure
{
    public partial class PostgresCacheDbContext : MixCacheDbContext
    {
        public PostgresCacheDbContext()
        {

        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyAllConfigurationsFromNamespace(
                this.GetType().Assembly,
                "Mix.Heart.Infrastructure.EntityConfigurations.POSTGRES");
            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
