using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Mix.Heart.Enums;
using Mix.Heart.Models;

namespace Mix.Heart.Entities.Cache
{
    public partial class MixCacheDbContext : DbContext
    {
        private readonly MixHeartConfigurationModel _configs;

        public MixCacheDbContext(IConfiguration configuration)
        {
            _configs = configuration.Get<MixHeartConfigurationModel>();
        }

        public virtual DbSet<MixCache> MixCache { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            //string cnn = MixCommonHelper.GetWebConfig<string>(WebConfiguration.MixCacheConnectionString);

            if (!string.IsNullOrEmpty(_configs.CacheConnection))
            {
                switch (_configs.DatabaseProvider)
                {
                    case MixDatabaseProvider.SQLSERVER:
                        optionsBuilder.UseSqlServer(_configs.CacheConnection);
                        break;

                    case MixDatabaseProvider.MySQL:
                        optionsBuilder.UseMySql(_configs.CacheConnection, ServerVersion.AutoDetect(_configs.CacheConnection));
                        break;

                    case MixDatabaseProvider.SQLITE:
                        optionsBuilder.UseSqlite(_configs.CacheConnection);
                        break;

                    case MixDatabaseProvider.PostgreSQL:
                        optionsBuilder.UseNpgsql(_configs.CacheConnection);
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
