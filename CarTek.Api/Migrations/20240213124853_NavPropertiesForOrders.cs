using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarTek.Api.Migrations
{
    /// <inheritdoc />
    public partial class NavPropertiesForOrders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_SubTasks_OrderId",
                table: "SubTasks",
                column: "OrderId");

            migrationBuilder.AddForeignKey(
                name: "FK_SubTasks_Orders_OrderId",
                table: "SubTasks",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SubTasks_Orders_OrderId",
                table: "SubTasks");

            migrationBuilder.DropIndex(
                name: "IX_SubTasks_OrderId",
                table: "SubTasks");
        }
    }
}
