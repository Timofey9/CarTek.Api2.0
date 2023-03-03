﻿// <auto-generated />
using System;
using CarTek.Api.DBContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CarTek.Api.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20230302090717_MultipleCarDrivers")]
    partial class MultipleCarDrivers
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.2")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("CarTek.Api.Model.Car", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<int>("AxelsCount")
                        .HasColumnType("integer");

                    b.Property<string>("Brand")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Model")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Plate")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("State")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.ToTable("cars", (string)null);
                });

            modelBuilder.Entity("CarTek.Api.Model.Driver", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<long?>("CarId")
                        .HasColumnType("bigint");

                    b.Property<string>("FirstName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("LastName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("MiddleName")
                        .HasColumnType("text");

                    b.Property<string>("Password")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Phone")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("CarId");

                    b.ToTable("drivers", (string)null);
                });

            modelBuilder.Entity("CarTek.Api.Model.Questionary", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<string>("Action")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime?>("ApprovedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<bool>("BackSuspension")
                        .HasColumnType("boolean");

                    b.Property<bool>("CabinCushion")
                        .HasColumnType("boolean");

                    b.Property<long?>("CarId")
                        .HasColumnType("bigint");

                    b.Property<string>("Comment")
                        .HasColumnType("text");

                    b.Property<long>("DriverId")
                        .HasColumnType("bigint");

                    b.Property<string>("FendersJsonObject")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<bool>("FrontSuspension")
                        .HasColumnType("boolean");

                    b.Property<bool>("GeneralCondition")
                        .HasColumnType("boolean");

                    b.Property<bool>("HydroEq")
                        .HasColumnType("boolean");

                    b.Property<string>("ImagesPath")
                        .HasColumnType("text");

                    b.Property<bool?>("IsCabinClean")
                        .HasColumnType("boolean");

                    b.Property<DateTime>("LastUpdated")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("LightsJsonObject")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int?>("Mileage")
                        .HasColumnType("integer");

                    b.Property<bool>("PlatonInPlace")
                        .HasColumnType("boolean");

                    b.Property<bool>("PlatonSwitchedOn")
                        .HasColumnType("boolean");

                    b.Property<bool>("Rack")
                        .HasColumnType("boolean");

                    b.Property<long?>("TrailerId")
                        .HasColumnType("bigint");

                    b.Property<string>("Type")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<Guid>("UniqueId")
                        .HasColumnType("uuid");

                    b.Property<long>("UserId")
                        .HasColumnType("bigint");

                    b.Property<bool?>("WasApproved")
                        .HasColumnType("boolean");

                    b.Property<string>("WheelsJsonObject")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("CarId");

                    b.HasIndex("DriverId");

                    b.HasIndex("TrailerId");

                    b.HasIndex("UserId");

                    b.ToTable("questionaries", (string)null);
                });

            modelBuilder.Entity("CarTek.Api.Model.Trailer", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<int>("AxelsCount")
                        .HasColumnType("integer");

                    b.Property<string>("Brand")
                        .HasColumnType("text");

                    b.Property<long?>("CarId")
                        .HasColumnType("bigint");

                    b.Property<string>("Model")
                        .HasColumnType("text");

                    b.Property<string>("Plate")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("CarId")
                        .IsUnique();

                    b.ToTable("Trailers");
                });

            modelBuilder.Entity("CarTek.Api.Model.User", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<string>("Email")
                        .HasColumnType("text");

                    b.Property<string>("FirstName")
                        .HasColumnType("text");

                    b.Property<bool>("IsAdmin")
                        .HasColumnType("boolean");

                    b.Property<string>("LastName")
                        .HasColumnType("text");

                    b.Property<string>("Login")
                        .HasColumnType("text");

                    b.Property<string>("MiddleName")
                        .HasColumnType("text");

                    b.Property<string>("Password")
                        .HasColumnType("text");

                    b.Property<string>("Phone")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("CarTek.Api.Model.Driver", b =>
                {
                    b.HasOne("CarTek.Api.Model.Car", "Car")
                        .WithMany("Drivers")
                        .HasForeignKey("CarId");

                    b.Navigation("Car");
                });

            modelBuilder.Entity("CarTek.Api.Model.Questionary", b =>
                {
                    b.HasOne("CarTek.Api.Model.Car", "Car")
                        .WithMany("Questionaries")
                        .HasForeignKey("CarId");

                    b.HasOne("CarTek.Api.Model.Driver", "Driver")
                        .WithMany("Questionaries")
                        .HasForeignKey("DriverId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("CarTek.Api.Model.Trailer", "Trailer")
                        .WithMany()
                        .HasForeignKey("TrailerId");

                    b.HasOne("CarTek.Api.Model.User", "User")
                        .WithMany("Questionaries")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Car");

                    b.Navigation("Driver");

                    b.Navigation("Trailer");

                    b.Navigation("User");
                });

            modelBuilder.Entity("CarTek.Api.Model.Trailer", b =>
                {
                    b.HasOne("CarTek.Api.Model.Car", "Car")
                        .WithOne("Trailer")
                        .HasForeignKey("CarTek.Api.Model.Trailer", "CarId");

                    b.Navigation("Car");
                });

            modelBuilder.Entity("CarTek.Api.Model.Car", b =>
                {
                    b.Navigation("Drivers");

                    b.Navigation("Questionaries");

                    b.Navigation("Trailer")
                        .IsRequired();
                });

            modelBuilder.Entity("CarTek.Api.Model.Driver", b =>
                {
                    b.Navigation("Questionaries");
                });

            modelBuilder.Entity("CarTek.Api.Model.User", b =>
                {
                    b.Navigation("Questionaries");
                });
#pragma warning restore 612, 618
        }
    }
}
