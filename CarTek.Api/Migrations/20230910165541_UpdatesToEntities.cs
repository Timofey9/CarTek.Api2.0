using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CarTek.Api.Migrations
{
    /// <inheritdoc />
    public partial class UpdatesToEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<double>(
                name: "Price",
                table: "Orders",
                type: "double precision",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "double precision");

            migrationBuilder.AlterColumn<int>(
                name: "Mileage",
                table: "Orders",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<long>(
                name: "GpId",
                table: "Orders",
                type: "bigint",
                nullable: true);

            migrationBuilder.AlterColumn<double>(
                name: "Volume",
                table: "DriverTasks",
                type: "double precision",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<long>(
                name: "TNId",
                table: "DriverTasks",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "TnId",
                table: "DriverTasks",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "TNs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Number = table.Column<string>(type: "text", nullable: true),
                    DriverTaskId = table.Column<long>(type: "bigint", nullable: false),
                    DriverId = table.Column<long>(type: "bigint", nullable: false),
                    LoadVolume = table.Column<double>(type: "double precision", nullable: true),
                    UnloadVolume = table.Column<double>(type: "double precision", nullable: true),
                    Unit = table.Column<int>(type: "integer", nullable: true),
                    GoId = table.Column<long>(type: "bigint", nullable: true),
                    GpId = table.Column<long>(type: "bigint", nullable: true),
                    LocationAId = table.Column<int>(type: "integer", nullable: true),
                    LocationBId = table.Column<int>(type: "integer", nullable: true),
                    PickUpDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PickUpArrivalTime = table.Column<string>(type: "text", nullable: true),
                    PickUpDepartureTime = table.Column<string>(type: "text", nullable: true),
                    DropOffDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DropOffArrivalTime = table.Column<string>(type: "text", nullable: true),
                    DropOffDepartureTime = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TNs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DriverTasks_TNId",
                table: "DriverTasks",
                column: "TNId");

            migrationBuilder.AddForeignKey(
                name: "FK_DriverTasks_TNs_TNId",
                table: "DriverTasks",
                column: "TNId",
                principalTable: "TNs",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DriverTasks_TNs_TNId",
                table: "DriverTasks");

            migrationBuilder.DropTable(
                name: "TNs");

            migrationBuilder.DropIndex(
                name: "IX_DriverTasks_TNId",
                table: "DriverTasks");

            migrationBuilder.DropColumn(
                name: "GpId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "TNId",
                table: "DriverTasks");

            migrationBuilder.DropColumn(
                name: "TnId",
                table: "DriverTasks");

            migrationBuilder.AlterColumn<double>(
                name: "Price",
                table: "Orders",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0,
                oldClrType: typeof(double),
                oldType: "double precision",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Mileage",
                table: "Orders",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Volume",
                table: "DriverTasks",
                type: "integer",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "double precision");
        }
    }
}
