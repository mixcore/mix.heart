using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mix.Example.Infrastructure.MixEntities;

namespace Mix.Example.Infrastructure.MixConfiguration
{
    public class ProductDetailEntityConfiguration : IEntityTypeConfiguration<ProductDetailEntity>
    {
        public void Configure(EntityTypeBuilder<ProductDetailEntity> builder)
        {
        }
    }
}
