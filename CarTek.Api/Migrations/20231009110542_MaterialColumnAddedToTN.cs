using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarTek.Api.Migrations
{
    /// <inheritdoc />
    public partial class MaterialColumnAddedToTN : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "LoadVolume2",
                table: "TNs",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "MaterialId",
                table: "TNs",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Unit2",
                table: "TNs",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "UnloadVolume2",
                table: "TNs",
                type: "double precision",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TNs_MaterialId",
                table: "TNs",
                column: "MaterialId");

            migrationBuilder.AddForeignKey(
                name: "FK_TNs_Materials_MaterialId",
                table: "TNs",
                column: "MaterialId",
                principalTable: "Materials",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TNs_Materials_MaterialId",
                table: "TNs");

            migrationBuilder.DropIndex(
                name: "IX_TNs_MaterialId",
                table: "TNs");

            migrationBuilder.DropColumn(
                name: "LoadVolume2",
                table: "TNs");

            migrationBuilder.DropColumn(
                name: "MaterialId",
                table: "TNs");

            migrationBuilder.DropColumn(
                name: "Unit2",
                table: "TNs");

            migrationBuilder.DropColumn(
                name: "UnloadVolume2",
                table: "TNs");
        }
    }
}
