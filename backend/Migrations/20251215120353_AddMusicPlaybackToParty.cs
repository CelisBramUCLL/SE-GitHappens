using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dotnet_test.Migrations
{
    /// <inheritdoc />
    public partial class AddMusicPlaybackToParty : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CurrentPosition",
                table: "Parties",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CurrentlyPlayingSongId",
                table: "Parties",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsPlaying",
                table: "Parties",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CurrentPosition",
                table: "Parties");

            migrationBuilder.DropColumn(
                name: "CurrentlyPlayingSongId",
                table: "Parties");

            migrationBuilder.DropColumn(
                name: "IsPlaying",
                table: "Parties");
        }
    }
}
