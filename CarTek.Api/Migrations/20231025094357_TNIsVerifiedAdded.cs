﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarTek.Api.Migrations
{
    /// <inheritdoc />
    public partial class TNIsVerifiedAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsVerified",
                table: "TNs",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsVerified",
                table: "TNs");
        }
    }
}
