using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarTek.Api.Migrations
{
    /// <inheritdoc />
    public partial class DeleteFKCarId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_questionaries_Users_UserId",
                table: "questionaries");

            migrationBuilder.DropForeignKey(
                name: "FK_questionaries_drivers_DriverId",
                table: "questionaries");

            migrationBuilder.AlterColumn<long>(
                name: "UserId",
                table: "questionaries",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<long>(
                name: "DriverId",
                table: "questionaries",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddForeignKey(
                name: "FK_questionaries_Users_UserId",
                table: "questionaries",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_questionaries_drivers_DriverId",
                table: "questionaries",
                column: "DriverId",
                principalTable: "drivers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_questionaries_Users_UserId",
                table: "questionaries");

            migrationBuilder.DropForeignKey(
                name: "FK_questionaries_drivers_DriverId",
                table: "questionaries");

            migrationBuilder.AlterColumn<long>(
                name: "UserId",
                table: "questionaries",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "DriverId",
                table: "questionaries",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_questionaries_Users_UserId",
                table: "questionaries",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_questionaries_drivers_DriverId",
                table: "questionaries",
                column: "DriverId",
                principalTable: "drivers",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);
        }
    }
}
