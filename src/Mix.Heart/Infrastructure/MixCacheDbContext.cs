using Microsoft.EntityFrameworkCore;
using Mix.Common.Helper;
using Mix.Heart.Constants;
using Mix.Heart.Infrastructure.Entities;

namespace Mix.Heart.Infrastructure.ViewModels
{
    public partial class MixCacheDbContext: DbContext
    {
        public virtual DbSet<MixCache> MixCache { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string cnn = CommonHelper.GetWebConfig<string>(WebConfiguration.MixCacheDb);
            optionsBuilder.UseSqlite($"Data Source={cnn ?? "mix_cache.db"}");
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(
                this.GetType().Assembly);
            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
