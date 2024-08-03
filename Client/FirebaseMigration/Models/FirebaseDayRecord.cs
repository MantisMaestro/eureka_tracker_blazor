namespace Client.FirebaseMigration.Models;

public class FirebaseDayRecord
{
    public string Date { get; set; } = null!;

    public List<FirebasePlayerSession> PlayerSessions { get; set; } = [];
}