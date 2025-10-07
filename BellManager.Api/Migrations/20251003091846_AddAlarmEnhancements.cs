using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BellManager.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddAlarmEnhancements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "church_id",
                table: "alarms",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_repeating",
                table: "alarms",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "repeat_type",
                table: "alarms",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "Once");

            migrationBuilder.AddColumn<DateTime>(
                name: "selected_date",
                table: "alarms",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_alarms_church_id",
                table: "alarms",
                column: "church_id");

            migrationBuilder.AddForeignKey(
                name: "FK_alarms_churches_church_id",
                table: "alarms",
                column: "church_id",
                principalTable: "churches",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_alarms_churches_church_id",
                table: "alarms");

            migrationBuilder.DropIndex(
                name: "IX_alarms_church_id",
                table: "alarms");

            migrationBuilder.DropColumn(
                name: "church_id",
                table: "alarms");

            migrationBuilder.DropColumn(
                name: "is_repeating",
                table: "alarms");

            migrationBuilder.DropColumn(
                name: "repeat_type",
                table: "alarms");

            migrationBuilder.DropColumn(
                name: "selected_date",
                table: "alarms");
        }
    }
}
