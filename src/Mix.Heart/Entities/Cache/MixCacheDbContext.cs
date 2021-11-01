using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Mix.Heart.Enums;
using Mix.Heart.Model;
using Newtonsoft.Json.Linq;

namespace Mix.Heart.Entities.Cache
{
    public partial class MixCacheDbContext: DbContext
    {
        private readonly IConfiguration _configuration;
        private MixHeartConfigurationModel _configs;

        public MixCacheDbContext(IConfiguration configuration)
        {
            _configuration = configuration;
            var settings = _configuration.GetSection("MixHeart");
            _configs = new MixHeartConfigurationModel();
            settings.Bind(_configs);
        }

        public virtual DbSet<MixCache> MixCache { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            //string cnn = MixCommonHelper.GetWebConfig<string>(WebConfiguration.MixCacheConnectionString);
            
            if (!string.IsNullOrEmpty(_configs.ConnectionString))
            {
                switch (_configs.DatabaseProvider)
                {
                    case MixCacheDbProvider.SQLSERVER:
                        optionsBuilder.UseSqlServer(_configs.ConnectionString);
                        break;

                    case MixCacheDbProvider.MYSQL:
                        optionsBuilder.UseMySql(_configs.ConnectionString, ServerVersion.AutoDetect(_configs.ConnectionString));
                        break;

                    case MixCacheDbProvider.SQLITE:
                        optionsBuilder.UseSqlite(_configs.ConnectionString);
                        break;

                    case MixCacheDbProvider.POSGRES:
                        optionsBuilder.UseNpgsql(_configs.ConnectionString);
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
