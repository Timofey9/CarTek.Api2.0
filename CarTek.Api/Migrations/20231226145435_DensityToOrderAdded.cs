using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarTek.Api.Migrations
{
    /// <inheritdoc />
    public partial class DensityToOrderAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Density",
                table: "Clients");

            migrationBuilder.AddColumn<double>(
                name: "Density",
                table: "Orders",
                type: "double precision",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Density",
                table: "Orders");

            migrationBuilder.AddColumn<double>(
                name: "Density",
                table: "Clients",
                type: "double precision",
                nullable: true);
        }
    }
}
