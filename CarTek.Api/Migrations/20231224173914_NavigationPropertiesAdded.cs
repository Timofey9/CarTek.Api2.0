using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarTek.Api.Migrations
{
    /// <inheritdoc />
    public partial class NavigationPropertiesAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<long>(
                name: "LocationBId",
                table: "TNs",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "LocationAId",
                table: "TNs",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TNs_LocationAId",
                table: "TNs",
                column: "LocationAId");

            migrationBuilder.CreateIndex(
                name: "IX_TNs_LocationBId",
                table: "TNs",
                column: "LocationBId");

            migrationBuilder.AddForeignKey(
                name: "FK_TNs_Addresses_LocationAId",
                table: "TNs",
                column: "LocationAId",
                principalTable: "Addresses",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TNs_Addresses_LocationBId",
                table: "TNs",
                column: "LocationBId",
                principalTable: "Addresses",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TNs_Addresses_LocationAId",
                table: "TNs");

            migrationBuilder.DropForeignKey(
                name: "FK_TNs_Addresses_LocationBId",
                table: "TNs");

            migrationBuilder.DropIndex(
                name: "IX_TNs_LocationAId",
                table: "TNs");

            migrationBuilder.DropIndex(
                name: "IX_TNs_LocationBId",
                table: "TNs");

            migrationBuilder.AlterColumn<int>(
                name: "LocationBId",
                table: "TNs",
                type: "integer",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "LocationAId",
                table: "TNs",
                type: "integer",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);
        }
    }
}
