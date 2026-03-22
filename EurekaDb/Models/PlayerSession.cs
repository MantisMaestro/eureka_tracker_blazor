namespace EurekaDb.Models;

public class PlayerSession
{
    public string PlayerId { get; set; } = null!;

    public DateOnly Date { get; set; }

    public int? TimePlayedInSession { get; set; }

    public virtual Player Player { get; set; } = null!;
}