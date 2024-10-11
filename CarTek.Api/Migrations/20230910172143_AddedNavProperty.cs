using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarTek.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddedNavProperty : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DriverTasks_TNs_TNId",
                table: "DriverTasks");

            migrationBuilder.DropIndex(
                name: "IX_DriverTasks_TNId",
                table: "DriverTasks");

            migrationBuilder.DropColumn(
                name: "TNId",
                table: "DriverTasks");

            migrationBuilder.DropColumn(
                name: "TnId",
                table: "DriverTasks");

            migrationBuilder.CreateIndex(
                name: "IX_TNs_DriverTaskId",
                table: "TNs",
                column: "DriverTaskId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_TNs_DriverTasks_DriverTaskId",
                table: "TNs",
                column: "DriverTaskId",
                principalTable: "DriverTasks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TNs_DriverTasks_DriverTaskId",
                table: "TNs");

            migrationBuilder.DropIndex(
                name: "IX_TNs_DriverTaskId",
                table: "TNs");

            migrationBuilder.AddColumn<long>(
                name: "TNId",
                table: "DriverTasks",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "TnId",
                table: "DriverTasks",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_DriverTasks_TNId",
                table: "DriverTasks",
                column: "TNId");

            migrationBuilder.AddForeignKey(
                name: "FK_DriverTasks_TNs_TNId",
                table: "DriverTasks",
                column: "TNId",
                principalTable: "TNs",
                principalColumn: "Id");
        }
    }
}
