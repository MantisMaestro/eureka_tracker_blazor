using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EurekaDb.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Commented out as this is already the deployed state
            // migrationBuilder.CreateTable(
            //     name: "players",
            //     columns: table => new
            //     {
            //         id = table.Column<string>(type: "TEXT", nullable: false),
            //         name = table.Column<string>(type: "TEXT", nullable: false),
            //         last_online = table.Column<string>(type: "TEXT", nullable: true),
            //         total_play_time = table.Column<int>(type: "INTEGER", nullable: true)
            //     },
            //     constraints: table =>
            //     {
            //         table.PrimaryKey("PK_players", x => x.id);
            //     });
            //
            // migrationBuilder.CreateTable(
            //     name: "player_sessions",
            //     columns: table => new
            //     {
            //         player_id = table.Column<string>(type: "TEXT", nullable: false),
            //         date = table.Column<DateOnly>(type: "TEXT", nullable: false),
            //         time_played_in_session = table.Column<int>(type: "INTEGER", nullable: true)
            //     },
            //     constraints: table =>
            //     {
            //         table.PrimaryKey("PK_player_sessions", x => new { x.player_id, x.date });
            //         table.ForeignKey(
            //             name: "FK_player_sessions_players_player_id",
            //             column: x => x.player_id,
            //             principalTable: "players",
            //             principalColumn: "id");
            //     });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "player_sessions");

            migrationBuilder.DropTable(
                name: "players");
        }
    }
}
