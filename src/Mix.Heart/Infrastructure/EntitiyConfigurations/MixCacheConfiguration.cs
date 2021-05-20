using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Mix.Common.Helper;
using Mix.Heart.Constants;
using Mix.Heart.Enums;
using Mix.Heart.Infrastructure.Entities;
using System.Text;

namespace Mix.Heart.Infrastructure.EntityConfigurations
{
    public class MixCacheConfiguration : IEntityTypeConfiguration<MixCache>
    {
        public void Configure(EntityTypeBuilder<MixCache> entity)
        {
            var dbProvider = CommonHelper.GetWebEnumConfig<MixDatabaseProvider>(WebConfiguration.MixCacheDbProvider);
            entity.ToTable("mix_cache");

            entity.HasIndex(e => e.ExpiredDateTime)
                .HasDatabaseName("Index_ExpiresAtTime");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnType("varchar(150)");

            entity.Property(e => e.CreatedBy)
                .HasColumnType("varchar(50)")
                .IsRequired(false);

            entity.Property(e => e.CreatedDateTime).HasColumnType("datetime");

            entity.Property(e => e.ExpiredDateTime).HasColumnType("datetime");

            entity.Property(e => e.LastModified).HasColumnType("datetime").IsRequired(false);

            entity.Property(e => e.ModifiedBy)
                .HasColumnType("varchar(50)")
                .IsRequired(false);

            entity.Property(e => e.Status)
                .IsRequired()
                .HasConversion(new EnumToStringConverter<MixCacheStatus>())
                .HasColumnType("varchar(50)");
            if (dbProvider == MixDatabaseProvider.MSSQL)
            {
                entity.Property(e => e.Value)
                .IsRequired()
                .HasColumnType("ntext");
            }
            else
            {
                entity.Property(e => e.Value)
                .IsRequired()
                .HasColumnType("text");
            }
        }
    }
}