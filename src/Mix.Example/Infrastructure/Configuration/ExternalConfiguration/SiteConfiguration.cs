using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mix.Example.Infrastructure.ExternalEntites;

namespace Mix.Example.Infrastructure.ExternalConfiguration
{
    public class SiteConfiguration : IEntityTypeConfiguration<SiteEntity>
    {
        public void Configure(EntityTypeBuilder<SiteEntity> builder)
        {
        }
    }
}
