using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dotnet_test.Migrations
{
    /// <inheritdoc />
    public partial class ConvertDurationToInt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // First, add a temporary column to store the converted values
            migrationBuilder.AddColumn<int>(
                name: "DurationTemp",
                table: "Songs",
                type: "int",
                nullable: false,
                defaultValue: 0
            );

            // Convert existing TimeSpan values to total seconds
            migrationBuilder.Sql(
                @"
                UPDATE Songs 
                SET DurationTemp = DATEPART(HOUR, Duration) * 3600 + 
                                  DATEPART(MINUTE, Duration) * 60 + 
                                  DATEPART(SECOND, Duration)"
            );

            // Drop the original Duration column
            migrationBuilder.DropColumn(name: "Duration", table: "Songs");

            // Rename the temporary column to Duration
            migrationBuilder.RenameColumn(
                name: "DurationTemp",
                table: "Songs",
                newName: "Duration"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Add temporary TimeSpan column
            migrationBuilder.AddColumn<TimeSpan>(
                name: "DurationTemp",
                table: "Songs",
                type: "time",
                nullable: false,
                defaultValue: TimeSpan.Zero
            );

            // Convert int values back to TimeSpan (this is lossy for values > 24 hours)
            migrationBuilder.Sql(
                @"
                UPDATE Songs 
                SET DurationTemp = CAST(DATEADD(SECOND, Duration, '00:00:00') AS TIME)"
            );

            // Drop the int Duration column
            migrationBuilder.DropColumn(name: "Duration", table: "Songs");

            // Rename temporary column back to Duration
            migrationBuilder.RenameColumn(
                name: "DurationTemp",
                table: "Songs",
                newName: "Duration"
            );
        }
    }
}
