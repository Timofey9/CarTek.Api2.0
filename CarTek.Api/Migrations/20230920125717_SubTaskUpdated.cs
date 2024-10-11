using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarTek.Api.Migrations
{
    /// <inheritdoc />
    public partial class SubTaskUpdated : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DriverTaskNotes_DriverTasks_DriverTaskId",
                table: "DriverTaskNotes");

            migrationBuilder.AddColumn<long>(
                name: "OrderId",
                table: "SubTasks",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AlterColumn<long>(
                name: "DriverTaskId",
                table: "DriverTaskNotes",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddForeignKey(
                name: "FK_DriverTaskNotes_DriverTasks_DriverTaskId",
                table: "DriverTaskNotes",
                column: "DriverTaskId",
                principalTable: "DriverTasks",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DriverTaskNotes_DriverTasks_DriverTaskId",
                table: "DriverTaskNotes");

            migrationBuilder.DropColumn(
                name: "OrderId",
                table: "SubTasks");

            migrationBuilder.AlterColumn<long>(
                name: "DriverTaskId",
                table: "DriverTaskNotes",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_DriverTaskNotes_DriverTasks_DriverTaskId",
                table: "DriverTaskNotes",
                column: "DriverTaskId",
                principalTable: "DriverTasks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
