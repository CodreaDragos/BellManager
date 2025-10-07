using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace BellManager.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddChurchesAndUserChurch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "church_id",
                table: "users",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "churches",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    phone_number = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_churches", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_users_church_id",
                table: "users",
                column: "church_id");

            migrationBuilder.AddForeignKey(
                name: "FK_users_churches_church_id",
                table: "users",
                column: "church_id",
                principalTable: "churches",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_users_churches_church_id",
                table: "users");

            migrationBuilder.DropTable(
                name: "churches");

            migrationBuilder.DropIndex(
                name: "IX_users_church_id",
                table: "users");

            migrationBuilder.DropColumn(
                name: "church_id",
                table: "users");
        }
    }
}
