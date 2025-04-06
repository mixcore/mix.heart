using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Mix.Heart.Entities.Cache;
using Mix.Heart.Enums;

namespace Mix.Heart.EntityConfigurations.MSSQL
{
    public class MixCacheConfiguration : IEntityTypeConfiguration<MixCache>
    {
        public void Configure(EntityTypeBuilder<MixCache> entity)
        {
            string valueType = "ntext";
            string dtType = "datetime";

            entity.HasIndex(e => e.ExpiredDateTime)
                .HasDatabaseName("index_expires_at_time");

            entity.Property(e => e.Keyword)
                .HasColumnName("keyword")
                 .HasColumnType("varchar(400)")
                 .IsRequired();

            entity.Property(e => e.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd();

            entity.Property(e => e.CreatedBy)
                .HasColumnName("created_by")
                .HasColumnType("varchar(50)")
                .IsRequired(false);

            entity.Property(e => e.CreatedDateTime)
                .HasColumnName("created_date_time")
                .HasColumnType(dtType);

            entity.Property(e => e.ExpiredDateTime)
                .HasColumnName("expired_date_time")
                .HasColumnType(dtType);

            entity.Property(e => e.LastModified)
                .HasColumnName("last_modified")
                .HasColumnType(dtType).IsRequired(false);

            entity.Property(e => e.ModifiedBy)
                .HasColumnName("modified_by")
                .HasColumnType("varchar(50)")
                .IsRequired(false);

            entity.Property(e => e.Status)
                .IsRequired()
                .HasColumnName("status")
                .HasConversion(new EnumToStringConverter<MixCacheStatus>())
                .HasColumnType("varchar(50)");

            entity.Property(e => e.Value)
            .IsRequired()
            .HasColumnName("value")
            .HasColumnType(valueType);
        }
    }
}