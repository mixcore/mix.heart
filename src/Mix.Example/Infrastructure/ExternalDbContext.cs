using Microsoft.EntityFrameworkCore;
using Mix.Example.Infrastructure.ExternalEntites;

namespace Mix.Example.Infrastructure {
  public class ExternalDbContext : DbContext {
    public ExternalDbContext() {}

    public ExternalDbContext(DbContextOptions<ExternalDbContext> options)
        : base(options) {}

    public DbSet<SiteEntity> Site { get; set; }

    protected override void
    OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
      optionsBuilder.UseSqlite("Data Source=mix-heart-example.db");
      base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder) {
      var mixDbConfigurationNamespace =
          "Mix.Example.Infrastructure.ExternalConfiguration";
      modelBuilder.ApplyConfigurationsFromAssembly(
          this.GetType().Assembly,
          p => p.Namespace == mixDbConfigurationNamespace);
      base.OnModelCreating(modelBuilder);
    }
  }
}
