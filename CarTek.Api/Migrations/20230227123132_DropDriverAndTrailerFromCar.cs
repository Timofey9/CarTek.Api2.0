using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarTek.Api.Migrations
{
    /// <inheritdoc />
    public partial class DropDriverAndTrailerFromCar : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DriverId",
                table: "cars");

            migrationBuilder.DropColumn(
                name: "TrailerId",
                table: "cars");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "DriverId",
                table: "cars",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "TrailerId",
                table: "cars",
                type: "bigint",
                nullable: true);
        }
    }
}
