﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Mix.Heart.Domain;

namespace Mix.Heart.Migrations
{
    [DbContext(typeof(MixCacheDbContext))]
    partial class MixCacheDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "5.0.1");

            modelBuilder.Entity("Mix.Heart.Domain.Entities.MixCache", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("varchar(50)")
                        .UseCollation("NOCASE");

                    b.Property<string>("CreatedBy")
                        .HasColumnType("varchar(50)")
                        .UseCollation("NOCASE");

                    b.Property<DateTime>("CreatedDateTime")
                        .HasColumnType("datetime");

                    b.Property<DateTime?>("ExpiredDateTime")
                        .HasColumnType("datetime");

                    b.Property<DateTime?>("LastModified")
                        .HasColumnType("datetime");

                    b.Property<string>("ModifiedBy")
                        .HasColumnType("varchar(50)")
                        .UseCollation("NOCASE");

                    b.Property<int>("Priority")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasColumnType("varchar(50)")
                        .UseCollation("NOCASE");

                    b.Property<byte[]>("Value")
                        .IsRequired()
                        .HasColumnType("BLOB")
                        .UseCollation("NOCASE");

                    b.HasKey("Id");

                    b.HasIndex("ExpiredDateTime")
                        .HasDatabaseName("Index_ExpiresAtTime");

                    b.ToTable("mix_cache");
                });
#pragma warning restore 612, 618
        }
    }
}
