using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarTek.Api.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedDatesTN : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PickUpDate",
                table: "TNs",
                newName: "PickUpDepartureDate");

            migrationBuilder.RenameColumn(
                name: "DropOffDate",
                table: "TNs",
                newName: "PickUpArrivalDate");

            migrationBuilder.AddColumn<DateTime>(
                name: "DropOffArrivalDate",
                table: "TNs",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DropOffDepartureDate",
                table: "TNs",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DropOffArrivalDate",
                table: "TNs");

            migrationBuilder.DropColumn(
                name: "DropOffDepartureDate",
                table: "TNs");

            migrationBuilder.RenameColumn(
                name: "PickUpDepartureDate",
                table: "TNs",
                newName: "PickUpDate");

            migrationBuilder.RenameColumn(
                name: "PickUpArrivalDate",
                table: "TNs",
                newName: "DropOffDate");
        }
    }
}
