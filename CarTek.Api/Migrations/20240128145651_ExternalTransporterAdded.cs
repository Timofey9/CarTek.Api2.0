using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CarTek.Api.Migrations
{
    /// <inheritdoc />
    public partial class ExternalTransporterAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "Discount",
                table: "Orders",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "DriverPrice",
                table: "Orders",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "ExternalPrice",
                table: "Orders",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "ExternalTransporterId",
                table: "Orders",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsExternal",
                table: "Orders",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<long>(
                name: "ExternalTransporterId",
                table: "drivers",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsExternal",
                table: "drivers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<long>(
                name: "ExternalTransporterId",
                table: "cars",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsExternal",
                table: "cars",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "ExternalTransporters",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExternalTransporters", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Orders_ExternalTransporterId",
                table: "Orders",
                column: "ExternalTransporterId");

            migrationBuilder.CreateIndex(
                name: "IX_drivers_ExternalTransporterId",
                table: "drivers",
                column: "ExternalTransporterId");

            migrationBuilder.CreateIndex(
                name: "IX_cars_ExternalTransporterId",
                table: "cars",
                column: "ExternalTransporterId");

            migrationBuilder.AddForeignKey(
                name: "FK_cars_ExternalTransporters_ExternalTransporterId",
                table: "cars",
                column: "ExternalTransporterId",
                principalTable: "ExternalTransporters",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_drivers_ExternalTransporters_ExternalTransporterId",
                table: "drivers",
                column: "ExternalTransporterId",
                principalTable: "ExternalTransporters",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_ExternalTransporters_ExternalTransporterId",
                table: "Orders",
                column: "ExternalTransporterId",
                principalTable: "ExternalTransporters",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_cars_ExternalTransporters_ExternalTransporterId",
                table: "cars");

            migrationBuilder.DropForeignKey(
                name: "FK_drivers_ExternalTransporters_ExternalTransporterId",
                table: "drivers");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_ExternalTransporters_ExternalTransporterId",
                table: "Orders");

            migrationBuilder.DropTable(
                name: "ExternalTransporters");

            migrationBuilder.DropIndex(
                name: "IX_Orders_ExternalTransporterId",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_drivers_ExternalTransporterId",
                table: "drivers");

            migrationBuilder.DropIndex(
                name: "IX_cars_ExternalTransporterId",
                table: "cars");

            migrationBuilder.DropColumn(
                name: "Discount",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "DriverPrice",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ExternalPrice",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ExternalTransporterId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "IsExternal",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ExternalTransporterId",
                table: "drivers");

            migrationBuilder.DropColumn(
                name: "IsExternal",
                table: "drivers");

            migrationBuilder.DropColumn(
                name: "ExternalTransporterId",
                table: "cars");

            migrationBuilder.DropColumn(
                name: "IsExternal",
                table: "cars");
        }
    }
}
