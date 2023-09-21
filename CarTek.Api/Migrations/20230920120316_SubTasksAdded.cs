using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CarTek.Api.Migrations
{
    /// <inheritdoc />
    public partial class SubTasksAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "SubTaskId",
                table: "TNs",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SubTasksCount",
                table: "DriverTasks",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<long>(
                name: "SubTaskId",
                table: "DriverTaskNotes",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "SubTasks",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DriverTaskId = table.Column<long>(type: "bigint", nullable: false),
                    SequenceNumber = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubTasks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SubTasks_DriverTasks_DriverTaskId",
                        column: x => x.DriverTaskId,
                        principalTable: "DriverTasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TNs_SubTaskId",
                table: "TNs",
                column: "SubTaskId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DriverTaskNotes_SubTaskId",
                table: "DriverTaskNotes",
                column: "SubTaskId");

            migrationBuilder.CreateIndex(
                name: "IX_SubTasks_DriverTaskId",
                table: "SubTasks",
                column: "DriverTaskId");

            migrationBuilder.AddForeignKey(
                name: "FK_DriverTaskNotes_SubTasks_SubTaskId",
                table: "DriverTaskNotes",
                column: "SubTaskId",
                principalTable: "SubTasks",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TNs_SubTasks_SubTaskId",
                table: "TNs",
                column: "SubTaskId",
                principalTable: "SubTasks",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DriverTaskNotes_SubTasks_SubTaskId",
                table: "DriverTaskNotes");

            migrationBuilder.DropForeignKey(
                name: "FK_TNs_SubTasks_SubTaskId",
                table: "TNs");

            migrationBuilder.DropTable(
                name: "SubTasks");

            migrationBuilder.DropIndex(
                name: "IX_TNs_SubTaskId",
                table: "TNs");

            migrationBuilder.DropIndex(
                name: "IX_DriverTaskNotes_SubTaskId",
                table: "DriverTaskNotes");

            migrationBuilder.DropColumn(
                name: "SubTaskId",
                table: "TNs");

            migrationBuilder.DropColumn(
                name: "SubTasksCount",
                table: "DriverTasks");

            migrationBuilder.DropColumn(
                name: "SubTaskId",
                table: "DriverTaskNotes");
        }
    }
}
