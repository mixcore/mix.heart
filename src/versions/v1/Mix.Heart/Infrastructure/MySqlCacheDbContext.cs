
using Microsoft.EntityFrameworkCore;
using Mix.Heart.Extensions;

namespace Mix.Heart.Infrastructure
{
    public partial class MySqlCacheDbContext : MixCacheDbContext
    {
        public MySqlCacheDbContext()
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
