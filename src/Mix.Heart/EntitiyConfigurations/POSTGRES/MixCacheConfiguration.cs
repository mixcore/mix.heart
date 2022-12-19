using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Mix.Heart.Entities.Cache;
using Mix.Heart.Enums;

namespace Mix.Heart.Infrastructure.EntityConfigurations.POSTGRES
{
    public class MixCacheConfiguration : IEntityTypeConfiguration<MixCache>
    {
        public void Configure(EntityTypeBuilder<MixCache> entity)
        {
            const string valueType = "text";
            const string dtType = "timestamp with time zone";

            entity.HasIndex(e => e.ExpiredDateTime)
                .HasDatabaseName("Index_ExpiresAtTime");

            entity.Property(e => e.Keyword)
                 .HasColumnType("varchar(400)")
                 .IsRequired();

            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd();

            entity.Property(e => e.CreatedBy)
                .HasColumnType("varchar(50)")
                .IsRequired(false);

            entity.Property(e => e.CreatedDateTime).HasColumnType(dtType);

            entity.Property(e => e.ExpiredDateTime).HasColumnType(dtType);

            entity.Property(e => e.LastModified).HasColumnType(dtType).IsRequired(false);

            entity.Property(e => e.ModifiedBy)
                .HasColumnType("varchar(50)")
                .IsRequired(false);

            entity.Property(e => e.Status)
                .IsRequired()
                .HasConversion(new EnumToStringConverter<MixCacheStatus>())
                .HasColumnType("varchar(50)");

            entity.Property(e => e.Value)
            .IsRequired()
            .HasColumnType(valueType);
        }
    }
}