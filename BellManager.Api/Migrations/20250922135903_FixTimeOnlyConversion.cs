using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BellManager.Api.Migrations
{
    /// <inheritdoc />
    public partial class FixTimeOnlyConversion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "hour_utc",
                table: "alarms",
                type: "text",
                nullable: false,
                oldClrType: typeof(TimeOnly),
                oldType: "time without time zone");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<TimeOnly>(
                name: "hour_utc",
                table: "alarms",
                type: "time without time zone",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");
        }
    }
}
