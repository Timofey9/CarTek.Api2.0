using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarTek.Api.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedCarModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Email",
                table: "Drivers");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DriverId",
                table: "cars");

            migrationBuilder.DropColumn(
                name: "TrailerId",
                table: "cars");

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Drivers",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
