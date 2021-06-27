using Microsoft.EntityFrameworkCore;
using Mix.Example.Infrastructure.MixEntities;

namespace Mix.Example.Infrastructure {
  public class MixDbContext : DbContext {
    public MixDbContext() {}

    public MixDbContext(DbContextOptions<MixDbContext> options)
        : base(options) {}

    public DbSet<ProductEntity> Product { get; set; }

    public DbSet<CategoryEntity> Category { get; set; }

    public DbSet<ProductDetailEntity> ProductDetail { get; set; }

    public DbSet<StoreEntity> Store { get; set; }

    protected override void
    OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
      optionsBuilder.UseSqlite("Data Source=mix-db-example.db");
      base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder) {
      var mixDbConfigurationNamespace =
          "Mix.Example.Infrastructure.MixConfiguration";
      modelBuilder.ApplyConfigurationsFromAssembly(
          this.GetType().Assembly,
          p => p.Namespace == mixDbConfigurationNamespace);
      base.OnModelCreating(modelBuilder);
    }
  }
}
