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
    [Migration("20230815094321_MileageToOrderAdded")]
    partial class MileageToOrderAdded
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.2")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("CarTek.Api.Model.Address", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<string>("Coordinates")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("TextAddress")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("Addresses");
                });

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

                    b.Property<long>("CurrentOrderId")
                        .HasColumnType("bigint");

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

            modelBuilder.Entity("CarTek.Api.Model.Client", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<string>("ClientAddress")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("ClientName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Inn")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Kpp")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Ogrn")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("Clients");
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

                    b.Property<string>("Login")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("MiddleName")
                        .HasColumnType("text");

                    b.Property<string>("Password")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Phone")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("CarId");

                    b.ToTable("drivers", (string)null);
                });

            modelBuilder.Entity("CarTek.Api.Model.Notification", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<DateTime>("DateSent")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<bool>("IsDriverNotification")
                        .HasColumnType("boolean");

                    b.Property<int>("NotificationType")
                        .HasColumnType("integer");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<long>("UserId")
                        .HasColumnType("bigint");

                    b.HasKey("Id");

                    b.ToTable("Notifications");
                });

            modelBuilder.Entity("CarTek.Api.Model.Order", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<int>("CarCount")
                        .HasColumnType("integer");

                    b.Property<long?>("ClientId")
                        .HasColumnType("bigint");

                    b.Property<string>("ClientName")
                        .HasColumnType("text");

                    b.Property<DateTime>("DueDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<bool>("IsComplete")
                        .HasColumnType("boolean");

                    b.Property<int>("LoadUnit")
                        .HasColumnType("integer");

                    b.Property<string>("LocationA")
                        .HasColumnType("text");

                    b.Property<long?>("LocationAId")
                        .HasColumnType("bigint");

                    b.Property<string>("LocationB")
                        .HasColumnType("text");

                    b.Property<long?>("LocationBId")
                        .HasColumnType("bigint");

                    b.Property<long>("MaterialId")
                        .HasColumnType("bigint");

                    b.Property<int>("Mileage")
                        .HasColumnType("integer");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Note")
                        .HasColumnType("text");

                    b.Property<double>("Price")
                        .HasColumnType("double precision");

                    b.Property<int>("Service")
                        .HasColumnType("integer");

                    b.Property<DateTime>("StartDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int>("UnloadUnit")
                        .HasColumnType("integer");

                    b.Property<double>("Volume")
                        .HasColumnType("double precision");

                    b.HasKey("Id");

                    b.HasIndex("ClientId");

                    b.HasIndex("MaterialId");

                    b.ToTable("Orders");
                });

            modelBuilder.Entity("CarTek.Api.Model.Orders.DriverTask", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<long>("CarId")
                        .HasColumnType("bigint");

                    b.Property<long>("DriverId")
                        .HasColumnType("bigint");

                    b.Property<DateTime>("LastUpdated")
                        .HasColumnType("timestamp with time zone");

                    b.Property<long>("OrderId")
                        .HasColumnType("bigint");

                    b.Property<int>("Shift")
                        .HasColumnType("integer");

                    b.Property<DateTime>("StartDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int>("Status")
                        .HasColumnType("integer");

                    b.Property<Guid>("UniqueId")
                        .HasColumnType("uuid");

                    b.Property<int>("Unit")
                        .HasColumnType("integer");

                    b.Property<int>("Volume")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("CarId");

                    b.HasIndex("DriverId");

                    b.HasIndex("OrderId");

                    b.ToTable("DriverTasks");
                });

            modelBuilder.Entity("CarTek.Api.Model.Orders.DriverTaskNote", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<DateTime>("DateCreated")
                        .HasColumnType("timestamp with time zone");

                    b.Property<long>("DriverTaskId")
                        .HasColumnType("bigint");

                    b.Property<string>("S3Links")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("Status")
                        .HasColumnType("integer");

                    b.Property<string>("Text")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("DriverTaskId");

                    b.ToTable("DriverTaskNotes");
                });

            modelBuilder.Entity("CarTek.Api.Model.Orders.Material", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("Materials");
                });

            modelBuilder.Entity("CarTek.Api.Model.Questionary", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<string>("AcceptanceComment")
                        .HasColumnType("text");

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

                    b.Property<long?>("DriverId")
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

                    b.Property<string>("TechnicalComment")
                        .HasColumnType("text");

                    b.Property<long?>("TrailerId")
                        .HasColumnType("bigint");

                    b.Property<string>("Type")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<Guid>("UniqueId")
                        .HasColumnType("uuid");

                    b.Property<long?>("UserId")
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
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<long?>("CarId")
                        .HasColumnType("bigint");

                    b.Property<string>("Model")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Plate")
                        .IsRequired()
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

            modelBuilder.Entity("CarTek.Api.Model.Order", b =>
                {
                    b.HasOne("CarTek.Api.Model.Client", "Client")
                        .WithMany("Orders")
                        .HasForeignKey("ClientId");

                    b.HasOne("CarTek.Api.Model.Orders.Material", "Material")
                        .WithMany("Orders")
                        .HasForeignKey("MaterialId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Client");

                    b.Navigation("Material");
                });

            modelBuilder.Entity("CarTek.Api.Model.Orders.DriverTask", b =>
                {
                    b.HasOne("CarTek.Api.Model.Car", "Car")
                        .WithMany("DriverTasks")
                        .HasForeignKey("CarId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("CarTek.Api.Model.Driver", "Driver")
                        .WithMany("DriverTasks")
                        .HasForeignKey("DriverId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("CarTek.Api.Model.Order", "Order")
                        .WithMany("DriverTasks")
                        .HasForeignKey("OrderId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Car");

                    b.Navigation("Driver");

                    b.Navigation("Order");
                });

            modelBuilder.Entity("CarTek.Api.Model.Orders.DriverTaskNote", b =>
                {
                    b.HasOne("CarTek.Api.Model.Orders.DriverTask", null)
                        .WithMany("Notes")
                        .HasForeignKey("DriverTaskId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("CarTek.Api.Model.Questionary", b =>
                {
                    b.HasOne("CarTek.Api.Model.Car", "Car")
                        .WithMany("Questionaries")
                        .HasForeignKey("CarId");

                    b.HasOne("CarTek.Api.Model.Driver", "Driver")
                        .WithMany("Questionaries")
                        .HasForeignKey("DriverId");

                    b.HasOne("CarTek.Api.Model.Trailer", "Trailer")
                        .WithMany()
                        .HasForeignKey("TrailerId");

                    b.HasOne("CarTek.Api.Model.User", "User")
                        .WithMany("Questionaries")
                        .HasForeignKey("UserId");

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
                    b.Navigation("DriverTasks");

                    b.Navigation("Drivers");

                    b.Navigation("Questionaries");

                    b.Navigation("Trailer");
                });

            modelBuilder.Entity("CarTek.Api.Model.Client", b =>
                {
                    b.Navigation("Orders");
                });

            modelBuilder.Entity("CarTek.Api.Model.Driver", b =>
                {
                    b.Navigation("DriverTasks");

                    b.Navigation("Questionaries");
                });

            modelBuilder.Entity("CarTek.Api.Model.Order", b =>
                {
                    b.Navigation("DriverTasks");
                });

            modelBuilder.Entity("CarTek.Api.Model.Orders.DriverTask", b =>
                {
                    b.Navigation("Notes");
                });

            modelBuilder.Entity("CarTek.Api.Model.Orders.Material", b =>
                {
                    b.Navigation("Orders");
                });

            modelBuilder.Entity("CarTek.Api.Model.User", b =>
                {
                    b.Navigation("Questionaries");
                });
#pragma warning restore 612, 618
        }
    }
}
