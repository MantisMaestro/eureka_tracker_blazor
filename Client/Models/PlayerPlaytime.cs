namespace Client.Models;

public class PlayerPlaytime
{
    public string PlayerId { get; set; } = null!;

    public string PlayerName { get; set; } = null!;

    public int Playtime { get; set; }
}