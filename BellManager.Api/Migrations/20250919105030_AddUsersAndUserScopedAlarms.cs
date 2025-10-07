using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace BellManager.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddUsersAndUserScopedAlarms : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "user_id",
                table: "alarms",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    username = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    password_hash = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    role = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.id);
                });

            // Seed a default user to attach existing alarms
            migrationBuilder.Sql(@"
                INSERT INTO users (email, username, password_hash, role)
                VALUES ('default@local', 'default', '$2a$11$C/1u2u6rj1bY8vQ2b5kT3uH0V3x1S3r3WmJr5yGJk5s5zXo6o6o6K', 'user')
                ON CONFLICT DO NOTHING;
            ");

            // Set all existing alarms to belong to the default user (id = 1)
            migrationBuilder.Sql(@"
                UPDATE alarms SET user_id = (SELECT id FROM users WHERE username = 'default' LIMIT 1)
                WHERE user_id = 0;
            ");

            migrationBuilder.CreateIndex(
                name: "IX_alarms_user_id",
                table: "alarms",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_users_email",
                table: "users",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_username",
                table: "users",
                column: "username",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_alarms_users_user_id",
                table: "alarms",
                column: "user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_alarms_users_user_id",
                table: "alarms");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropIndex(
                name: "IX_alarms_user_id",
                table: "alarms");

            migrationBuilder.DropColumn(
                name: "user_id",
                table: "alarms");
        }
    }
}
