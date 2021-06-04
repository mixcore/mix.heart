using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Mix.Heart.Domain.Entities;
using Mix.Heart.Domain.Enums;
using System.Text;

namespace Mix.Heart.Domain.EntityConfigurations
{
    public class MixCacheConfiguration : IEntityTypeConfiguration<MixCache>
    {
        public void Configure(EntityTypeBuilder<MixCache> entity)
        {
            entity.ToTable("mix_cache");

            entity.HasIndex(e => e.ExpiredDateTime)
                .HasDatabaseName("Index_ExpiresAtTime");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnType("varchar(50)")
                .UseCollation("NOCASE");

            entity.Property(e => e.CreatedBy)
                .HasColumnType("varchar(50)")
                .UseCollation("NOCASE");

            entity.Property(e => e.CreatedDateTime).HasColumnType("datetime");

            entity.Property(e => e.ExpiredDateTime).HasColumnType("datetime");

            entity.Property(e => e.LastModified).HasColumnType("datetime");

            entity.Property(e => e.ModifiedBy)
                .HasColumnType("varchar(50)")
                .UseCollation("NOCASE");

            entity.Property(e => e.Status)
                .IsRequired()
                .HasConversion(new EnumToStringConverter<MixCacheStatus>())
                .HasColumnType("varchar(50)")
                .UseCollation("NOCASE");

            entity.Property(e => e.Value)
                .IsRequired()
                .HasConversion(new StringToBytesConverter(Encoding.Unicode))
                .HasColumnType("BLOB")
                .UseCollation("NOCASE");
        }
    }
}