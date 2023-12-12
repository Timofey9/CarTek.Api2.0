using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarTek.Api.Migrations
{
    /// <inheritdoc />
    public partial class CanceledFlagAddedToTAsk : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsCanceled",
                table: "DriverTasks",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsCanceled",
                table: "DriverTasks");
        }
    }
}
