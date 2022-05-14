﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Mix.Heart.Entities.Cache;

#nullable disable

namespace Mix.Heart.Migrations
{
    [DbContext(typeof(MySqlCacheDbContext))]
    partial class MySqlCacheDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.4")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            modelBuilder.Entity("Mix.Heart.Entities.Cache.MixCache", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("char(36)");

                    b.Property<string>("CreatedBy")
                        .HasColumnType("varchar(50)");

                    b.Property<DateTime>("CreatedDateTime")
                        .HasColumnType("datetime");

                    b.Property<DateTime?>("ExpiredDateTime")
                        .HasColumnType("datetime");

                    b.Property<string>("Keyword")
                        .IsRequired()
                        .HasColumnType("varchar(400)");

                    b.Property<DateTime?>("LastModified")
                        .HasColumnType("datetime");

                    b.Property<string>("ModifiedBy")
                        .HasColumnType("varchar(50)");

                    b.Property<int>("Priority")
                        .HasColumnType("int");

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasColumnType("varchar(50)");

                    b.Property<string>("Value")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("ExpiredDateTime")
                        .HasDatabaseName("Index_ExpiresAtTime");

                    b.ToTable("MixCache");
                });
#pragma warning restore 612, 618
        }
    }
}
