﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Mix.Heart.Entities.Cache;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Mix.Heart.Migrations.PostgreSql
{
[DbContext(typeof(PostgresCacheDbContext))]
partial class PostgresCacheDbContextModelSnapshot : ModelSnapshot
{
    protected override void BuildModel(ModelBuilder modelBuilder)
    {
#pragma warning disable 612, 618
        modelBuilder
        .HasAnnotation("ProductVersion", "7.0.0")
        .HasAnnotation("Relational:MaxIdentifierLength", 63);

        NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

        modelBuilder.Entity("Mix.Heart.Entities.Cache.MixCache", b =>
        {
            b.Property<Guid>("Id")
            .ValueGeneratedOnAdd()
            .HasColumnType("uuid");

            b.Property<string>("CreatedBy")
            .HasColumnType("varchar(50)");

            b.Property<DateTime>("CreatedDateTime")
            .HasColumnType("timestamp with time zone");

            b.Property<DateTime?>("ExpiredDateTime")
            .HasColumnType("timestamp with time zone");

            b.Property<string>("Keyword")
            .IsRequired()
            .HasColumnType("varchar(400)");

            b.Property<DateTime?>("LastModified")
            .HasColumnType("timestamp with time zone");

            b.Property<string>("ModifiedBy")
            .HasColumnType("varchar(50)");

            b.Property<int>("Priority")
            .HasColumnType("integer");

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
