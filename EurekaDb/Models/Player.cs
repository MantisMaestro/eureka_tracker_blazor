namespace EurekaDb.Migrations;

public class Player
{
    public string Id { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string? LastOnline { get; set; }

    public int? TotalPlayTime { get; set; }

    public virtual ICollection<PlayerSession> PlayerSessions { get; set; } = new List<PlayerSession>();
}