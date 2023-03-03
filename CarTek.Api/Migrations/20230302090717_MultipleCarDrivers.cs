using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarTek.Api.Migrations
{
    /// <inheritdoc />
    public partial class MultipleCarDrivers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Drivers_cars_CarId",
                table: "Drivers");

            migrationBuilder.DropForeignKey(
                name: "FK_questionaries_Drivers_DriverId",
                table: "questionaries");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Drivers",
                table: "Drivers");

            migrationBuilder.DropIndex(
                name: "IX_Drivers_CarId",
                table: "Drivers");

            migrationBuilder.RenameTable(
                name: "Drivers",
                newName: "drivers");

            migrationBuilder.AddPrimaryKey(
                name: "PK_drivers",
                table: "drivers",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_drivers_CarId",
                table: "drivers",
                column: "CarId");

            migrationBuilder.AddForeignKey(
                name: "FK_drivers_cars_CarId",
                table: "drivers",
                column: "CarId",
                principalTable: "cars",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_questionaries_drivers_DriverId",
                table: "questionaries",
                column: "DriverId",
                principalTable: "drivers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_drivers_cars_CarId",
                table: "drivers");

            migrationBuilder.DropForeignKey(
                name: "FK_questionaries_drivers_DriverId",
                table: "questionaries");

            migrationBuilder.DropPrimaryKey(
                name: "PK_drivers",
                table: "drivers");

            migrationBuilder.DropIndex(
                name: "IX_drivers_CarId",
                table: "drivers");

            migrationBuilder.RenameTable(
                name: "drivers",
                newName: "Drivers");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Drivers",
                table: "Drivers",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Drivers_CarId",
                table: "Drivers",
                column: "CarId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Drivers_cars_CarId",
                table: "Drivers",
                column: "CarId",
                principalTable: "cars",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_questionaries_Drivers_DriverId",
                table: "questionaries",
                column: "DriverId",
                principalTable: "Drivers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
