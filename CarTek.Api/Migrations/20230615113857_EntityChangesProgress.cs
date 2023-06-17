using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarTek.Api.Migrations
{
    /// <inheritdoc />
    public partial class EntityChangesProgress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_cars_CarId",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_CarId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "CarId",
                table: "Orders");

            migrationBuilder.RenameColumn(
                name: "DriverTaskStatusId",
                table: "DriverTasks",
                newName: "Volume");

            migrationBuilder.AddColumn<DateTime>(
                name: "StartDate",
                table: "DriverTasks",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "Unit",
                table: "DriverTasks",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_DriverTasks_CarId",
                table: "DriverTasks",
                column: "CarId");

            migrationBuilder.AddForeignKey(
                name: "FK_DriverTasks_cars_CarId",
                table: "DriverTasks",
                column: "CarId",
                principalTable: "cars",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DriverTasks_cars_CarId",
                table: "DriverTasks");

            migrationBuilder.DropIndex(
                name: "IX_DriverTasks_CarId",
                table: "DriverTasks");

            migrationBuilder.DropColumn(
                name: "StartDate",
                table: "DriverTasks");

            migrationBuilder.DropColumn(
                name: "Unit",
                table: "DriverTasks");

            migrationBuilder.RenameColumn(
                name: "Volume",
                table: "DriverTasks",
                newName: "DriverTaskStatusId");

            migrationBuilder.AddColumn<long>(
                name: "CarId",
                table: "Orders",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Orders_CarId",
                table: "Orders",
                column: "CarId");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_cars_CarId",
                table: "Orders",
                column: "CarId",
                principalTable: "cars",
                principalColumn: "Id");
        }
    }
}
