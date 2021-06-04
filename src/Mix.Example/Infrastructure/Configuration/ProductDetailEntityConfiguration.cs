using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mix.Example.Infrastructure.Entities;

namespace Mix.Example.Infrastructure.Configuration
{
    public class ProductDetailEntityConfiguration : IEntityTypeConfiguration<ProductDetailEntity>
    {
        public void Configure(EntityTypeBuilder<ProductDetailEntity> builder)
        {
        }
    }
}
