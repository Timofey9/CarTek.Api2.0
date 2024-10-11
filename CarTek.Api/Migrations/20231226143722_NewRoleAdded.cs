﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarTek.Api.Migrations
{
    /// <inheritdoc />
    public partial class NewRoleAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsLogistManager",
                table: "Users",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsLogistManager",
                table: "Users");
        }
    }
}
