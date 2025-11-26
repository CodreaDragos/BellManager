using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BellManager.Api.Migrations
{
    /// <inheritdoc />
    public partial class ChangeDaysOfWeekToArray : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Use raw SQL to handle the conversion from string (comma separated) to text[]
            migrationBuilder.Sql("ALTER TABLE alarms ALTER COLUMN days_of_week TYPE text[] USING string_to_array(days_of_week, ',');");
            
            // We still need to update the model metadata if we were using AlterColumn, 
            // but since we are using raw SQL, EF Core's snapshot will be updated by the fact that we have this migration 
            // and the snapshot matches the code.
            // However, to be safe and consistent with EF Core's tracking, we can leave the AlterColumn 
            // BUT AlterColumn generates the SQL. We can't easily mix them for the SAME column operation 
            // without EF Core trying to do it its way.
            // So we just use Sql. The Down method needs to reverse it.
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Convert back to string (comma separated)
            migrationBuilder.Sql("ALTER TABLE alarms ALTER COLUMN days_of_week TYPE character varying(64) USING array_to_string(days_of_week, ',');");
        }
    }
}
