using Microsoft.EntityFrameworkCore;
using Mix.Example.Infrastructure.Entities;

namespace Mix.Example.Infrastructure
{
    public class MixDbContext : DbContext
    {
        public MixDbContext()
        {
        }

        public MixDbContext(DbContextOptions<MixDbContext> options)
        {
        }

        public DbSet<ProductEntity> Product { get; set; }

        public DbSet<CategoryEntity> Category { get; set; }

        public DbSet<ProductDetailEntity> ProductDetail { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("Server=.;Database=MixExample;Trusted_Connection=True;");
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(this.GetType().Assembly);
            base.OnModelCreating(modelBuilder);
        }
    }
}
