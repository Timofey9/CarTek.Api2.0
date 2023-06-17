using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CarTek.Api.Migrations
{
    /// <inheritdoc />
    public partial class DriverNavPropertyToOrderAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DriverTasks_Orders_OrderId",
                table: "DriverTasks");

            migrationBuilder.DropIndex(
                name: "IX_DriverTasks_OrderId",
                table: "DriverTasks");

            migrationBuilder.AddColumn<long>(
                name: "DriverTaskOrderModelId",
                table: "DriverTaskNotes",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "DriverCarModel",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Plate = table.Column<string>(type: "text", nullable: false),
                    Brand = table.Column<string>(type: "text", nullable: false),
                    Model = table.Column<string>(type: "text", nullable: false),
                    Mileage = table.Column<int>(type: "integer", nullable: false),
                    AxelsCount = table.Column<int>(type: "integer", nullable: false),
                    State = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DriverCarModel", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DriverModel",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FirstName = table.Column<string>(type: "text", nullable: false),
                    MiddleName = table.Column<string>(type: "text", nullable: false),
                    LastName = table.Column<string>(type: "text", nullable: false),
                    Password = table.Column<string>(type: "text", nullable: false),
                    Phone = table.Column<string>(type: "text", nullable: false),
                    CarId = table.Column<long>(type: "bigint", nullable: true),
                    CarName = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DriverModel", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DriverModel_DriverCarModel_CarId",
                        column: x => x.CarId,
                        principalTable: "DriverCarModel",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "DriverTaskOrderModel",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UniqueId = table.Column<Guid>(type: "uuid", nullable: false),
                    Shift = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Volume = table.Column<int>(type: "integer", nullable: false),
                    Unit = table.Column<int>(type: "integer", nullable: false),
                    OrderId = table.Column<long>(type: "bigint", nullable: false),
                    DriverId = table.Column<long>(type: "bigint", nullable: false),
                    CarId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DriverTaskOrderModel", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DriverTaskOrderModel_DriverModel_DriverId",
                        column: x => x.DriverId,
                        principalTable: "DriverModel",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DriverTaskOrderModel_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DriverTaskNotes_DriverTaskOrderModelId",
                table: "DriverTaskNotes",
                column: "DriverTaskOrderModelId");

            migrationBuilder.CreateIndex(
                name: "IX_DriverModel_CarId",
                table: "DriverModel",
                column: "CarId");

            migrationBuilder.CreateIndex(
                name: "IX_DriverTaskOrderModel_DriverId",
                table: "DriverTaskOrderModel",
                column: "DriverId");

            migrationBuilder.CreateIndex(
                name: "IX_DriverTaskOrderModel_OrderId",
                table: "DriverTaskOrderModel",
                column: "OrderId");

            migrationBuilder.AddForeignKey(
                name: "FK_DriverTaskNotes_DriverTaskOrderModel_DriverTaskOrderModelId",
                table: "DriverTaskNotes",
                column: "DriverTaskOrderModelId",
                principalTable: "DriverTaskOrderModel",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DriverTaskNotes_DriverTaskOrderModel_DriverTaskOrderModelId",
                table: "DriverTaskNotes");

            migrationBuilder.DropTable(
                name: "DriverTaskOrderModel");

            migrationBuilder.DropTable(
                name: "DriverModel");

            migrationBuilder.DropTable(
                name: "DriverCarModel");

            migrationBuilder.DropIndex(
                name: "IX_DriverTaskNotes_DriverTaskOrderModelId",
                table: "DriverTaskNotes");

            migrationBuilder.DropColumn(
                name: "DriverTaskOrderModelId",
                table: "DriverTaskNotes");

            migrationBuilder.CreateIndex(
                name: "IX_DriverTasks_OrderId",
                table: "DriverTasks",
                column: "OrderId");

            migrationBuilder.AddForeignKey(
                name: "FK_DriverTasks_Orders_OrderId",
                table: "DriverTasks",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
