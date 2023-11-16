using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarTek.Api.Migrations
{
    /// <inheritdoc />
    public partial class FixedPriceAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Transporter",
                table: "TNs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "FixedPrice",
                table: "Clients",
                type: "double precision",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Transporter",
                table: "TNs");

            migrationBuilder.DropColumn(
                name: "FixedPrice",
                table: "Clients");
        }
    }
}
