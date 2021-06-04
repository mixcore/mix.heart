using Microsoft.EntityFrameworkCore;
using Mix.Common.Helper;
using Mix.Heart.Constants;
using Mix.Heart.Enums;
using Mix.Heart.Infrastructure.Entities;

namespace Mix.Heart.Infrastructure
{
    public partial class MixCacheDbContext: DbContext
    {
        public virtual DbSet<MixCache> MixCache { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string cnn = MixCommonHelper.GetWebConfig<string>(WebConfiguration.MixCacheConnectionString);
            if (!string.IsNullOrEmpty(cnn))
            {
                var provider = MixCommonHelper.GetWebEnumConfig<MixDatabaseProvider>(WebConfiguration.MixCacheDbProvider);
                switch (provider)
                {
                    case MixDatabaseProvider.MSSQL:
                        optionsBuilder.UseSqlServer(cnn);
                        break;

                    case MixDatabaseProvider.MySQL:
                        optionsBuilder.UseMySql(cnn, ServerVersion.AutoDetect(cnn));
                        break;

                    case MixDatabaseProvider.SQLITE:
                        optionsBuilder.UseSqlite(cnn);
                        break;

                    case MixDatabaseProvider.PostgreSQL:
                        optionsBuilder.UseNpgsql(cnn);
                        break;

                    default:
                        break;
                }
            }
            else
            {
                optionsBuilder.UseSqlite($"Data Source=MixContent\\mix_cache.db");
            }

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
