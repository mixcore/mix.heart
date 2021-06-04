using Microsoft.EntityFrameworkCore;
using Mix.Common.Helper;
using Mix.Heart.Domain.Entities;
using static Mix.Heart.Domain.Constants.Common;

namespace Mix.Heart.Domain
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
