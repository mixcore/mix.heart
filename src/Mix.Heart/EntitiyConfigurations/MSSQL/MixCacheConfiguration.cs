using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Mix.Heart.Enums;
using Mix.Heart.Entities.Cache;

namespace Mix.Heart.EntityConfigurations.MSSQL
{
public class MixCacheConfiguration : IEntityTypeConfiguration<MixCache>
{
    public void Configure(EntityTypeBuilder<MixCache> entity)
    {
        string valueType = "ntext";
        string dtType = "datetime";

        entity.ToTable("mix_cache");

        entity.HasIndex(e => e.ExpiredDateTime)
        .HasDatabaseName("Index_ExpiresAtTime");

        entity.Property(e => e.Id)
        .ValueGeneratedNever()
        .HasColumnType("varchar(150)");

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