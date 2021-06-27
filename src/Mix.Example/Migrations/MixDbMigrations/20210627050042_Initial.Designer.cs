﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Mix.Example.Infrastructure;

namespace Mix.Example.Migrations
{
[DbContext(typeof(MixDbContext))]
[Migration("20210627050042_Initial")]
partial class Initial
{
    protected override void BuildTargetModel(ModelBuilder modelBuilder)
    {
#pragma warning disable 612, 618
        modelBuilder
        .HasAnnotation("ProductVersion", "5.0.6");

        modelBuilder.Entity("Mix.Example.Infrastructure.MixEntities.CategoryEntity", b =>
        {
            b.Property<Guid>("Id")
            .ValueGeneratedOnAdd()
            .HasColumnType("TEXT");

            b.Property<string>("Description")
            .HasColumnType("TEXT");

            b.Property<string>("Name")
            .HasColumnType("TEXT");

            b.HasKey("Id");

            b.ToTable("Category");
        });

        modelBuilder.Entity("Mix.Example.Infrastructure.MixEntities.ProductDetailEntity", b =>
        {
            b.Property<Guid>("Id")
            .ValueGeneratedOnAdd()
            .HasColumnType("TEXT");

            b.Property<string>("Description")
            .HasColumnType("TEXT");

            b.Property<int>("InventoryNumber")
            .HasColumnType("INTEGER");

            b.Property<string>("Name")
            .HasColumnType("TEXT");

            b.Property<Guid>("ProductId")
            .HasColumnType("TEXT");

            b.Property<int>("Quantity")
            .HasColumnType("INTEGER");

            b.HasKey("Id");

            b.ToTable("ProductDetail");
        });

        modelBuilder.Entity("Mix.Example.Infrastructure.MixEntities.ProductEntity", b =>
        {
            b.Property<Guid>("Id")
            .ValueGeneratedOnAdd()
            .HasColumnType("TEXT");

            b.Property<Guid>("CategoryId")
            .HasColumnType("TEXT");

            b.Property<string>("Description")
            .HasColumnType("TEXT");

            b.Property<string>("Name")
            .HasColumnType("TEXT");

            b.Property<string>("Producer")
            .HasColumnType("TEXT");

            b.HasKey("Id");

            b.ToTable("Product");
        });

        modelBuilder.Entity("Mix.Example.Infrastructure.MixEntities.StoreEntity", b =>
        {
            b.Property<Guid>("Id")
            .ValueGeneratedOnAdd()
            .HasColumnType("TEXT");

            b.Property<string>("Address")
            .HasColumnType("TEXT");

            b.Property<string>("Country")
            .HasColumnType("TEXT");

            b.Property<string>("Description")
            .HasColumnType("TEXT");

            b.Property<string>("Name")
            .HasColumnType("TEXT");

            b.HasKey("Id");

            b.ToTable("Store");
        });
#pragma warning restore 612, 618
    }
}
}
