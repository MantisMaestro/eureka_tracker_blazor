using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EurekaDb.Migrations
{
    /// <inheritdoc />
    public partial class LastOnlineToDateTime : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                    name: "last_online_temp",
                    table: "players",
                    type: "TEXT",
                  nullable: true);
            
            migrationBuilder.Sql(@"
            UPDATE players
            SET last_online_temp = CASE
                WHEN last_online = 'now' THEN datetime('now')
                WHEN last_online IS NOT NULL THEN datetime(last_online)
                ELSE NULL
            END");
                 
            migrationBuilder.DropColumn(name: "last_online", table: "players");
            migrationBuilder.RenameColumn(name: "last_online_temp", table: "players", newName: "last_online");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
