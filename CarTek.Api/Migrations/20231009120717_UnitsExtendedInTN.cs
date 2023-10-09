using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarTek.Api.Migrations
{
    /// <inheritdoc />
    public partial class UnitsExtendedInTN : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UnloadUnit",
                table: "TNs",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UnloadUnit2",
                table: "TNs",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UnloadUnit",
                table: "TNs");

            migrationBuilder.DropColumn(
                name: "UnloadUnit2",
                table: "TNs");
        }
    }
}
