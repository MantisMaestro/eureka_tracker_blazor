using EurekaDb.Models;

namespace Client.Models;

public class PlayerQuery
{
    public List<PlayerSession> PlayerSessions { get; set; } = [];

    public int TotalPlaytime { get; set; }
}
