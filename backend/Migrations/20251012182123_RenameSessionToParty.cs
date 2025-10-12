using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dotnet_test.Migrations
{
    /// <inheritdoc />
    public partial class RenameSessionToParty : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Participants_Sessions_SessionId",
                table: "Participants");

            migrationBuilder.DropForeignKey(
                name: "FK_Playlists_Sessions_SessionId",
                table: "Playlists");

            migrationBuilder.DropTable(
                name: "Sessions");

            migrationBuilder.RenameColumn(
                name: "SessionId",
                table: "Playlists",
                newName: "PartyId");

            migrationBuilder.RenameIndex(
                name: "IX_Playlists_SessionId",
                table: "Playlists",
                newName: "IX_Playlists_PartyId");

            migrationBuilder.RenameColumn(
                name: "SessionId",
                table: "Participants",
                newName: "PartyId");

            migrationBuilder.RenameIndex(
                name: "IX_Participants_SessionId",
                table: "Participants",
                newName: "IX_Participants_PartyId");

            migrationBuilder.CreateTable(
                name: "Parties",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    HostUserId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Parties", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Parties_Users_HostUserId",
                        column: x => x.HostUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Parties_HostUserId",
                table: "Parties",
                column: "HostUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Participants_Parties_PartyId",
                table: "Participants",
                column: "PartyId",
                principalTable: "Parties",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Playlists_Parties_PartyId",
                table: "Playlists",
                column: "PartyId",
                principalTable: "Parties",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Participants_Parties_PartyId",
                table: "Participants");

            migrationBuilder.DropForeignKey(
                name: "FK_Playlists_Parties_PartyId",
                table: "Playlists");

            migrationBuilder.DropTable(
                name: "Parties");

            migrationBuilder.RenameColumn(
                name: "PartyId",
                table: "Playlists",
                newName: "SessionId");

            migrationBuilder.RenameIndex(
                name: "IX_Playlists_PartyId",
                table: "Playlists",
                newName: "IX_Playlists_SessionId");

            migrationBuilder.RenameColumn(
                name: "PartyId",
                table: "Participants",
                newName: "SessionId");

            migrationBuilder.RenameIndex(
                name: "IX_Participants_PartyId",
                table: "Participants",
                newName: "IX_Participants_SessionId");

            migrationBuilder.CreateTable(
                name: "Sessions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HostUserId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Sessions_Users_HostUserId",
                        column: x => x.HostUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_HostUserId",
                table: "Sessions",
                column: "HostUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Participants_Sessions_SessionId",
                table: "Participants",
                column: "SessionId",
                principalTable: "Sessions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Playlists_Sessions_SessionId",
                table: "Playlists",
                column: "SessionId",
                principalTable: "Sessions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
