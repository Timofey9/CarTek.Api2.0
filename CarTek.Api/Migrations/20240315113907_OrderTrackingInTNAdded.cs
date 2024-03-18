using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarTek.Api.Migrations
{
    /// <inheritdoc />
    public partial class OrderTrackingInTNAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "OrderId",
                table: "TNs",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "CreatedByDriver",
                table: "DriverTaskNotes",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_TNs_OrderId",
                table: "TNs",
                column: "OrderId");

            migrationBuilder.AddForeignKey(
                name: "FK_TNs_Orders_OrderId",
                table: "TNs",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TNs_Orders_OrderId",
                table: "TNs");

            migrationBuilder.DropIndex(
                name: "IX_TNs_OrderId",
                table: "TNs");

            migrationBuilder.DropColumn(
                name: "OrderId",
                table: "TNs");

            migrationBuilder.DropColumn(
                name: "CreatedByDriver",
                table: "DriverTaskNotes");
        }
    }
}
