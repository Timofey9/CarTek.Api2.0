﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarTek.Api.Migrations
{
    /// <inheritdoc />
    public partial class NoteNullability : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TNs_DriverTasks_DriverTaskId",
                table: "TNs");

            migrationBuilder.AlterColumn<long>(
                name: "DriverTaskId",
                table: "TNs",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddForeignKey(
                name: "FK_TNs_DriverTasks_DriverTaskId",
                table: "TNs",
                column: "DriverTaskId",
                principalTable: "DriverTasks",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TNs_DriverTasks_DriverTaskId",
                table: "TNs");

            migrationBuilder.AlterColumn<long>(
                name: "DriverTaskId",
                table: "TNs",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_TNs_DriverTasks_DriverTaskId",
                table: "TNs",
                column: "DriverTaskId",
                principalTable: "DriverTasks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
