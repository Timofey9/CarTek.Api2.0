using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarTek.Api.Migrations
{
    /// <inheritdoc />
    public partial class EntitiesEnriched : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "TransporterId",
                table: "TNs",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "MaterialPrice",
                table: "Orders",
                type: "double precision",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TransporterId",
                table: "TNs");

            migrationBuilder.DropColumn(
                name: "MaterialPrice",
                table: "Orders");
        }
    }
}
