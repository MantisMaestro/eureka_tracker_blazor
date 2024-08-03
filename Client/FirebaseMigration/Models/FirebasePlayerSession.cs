namespace Client.FirebaseMigration.Models;

public class FirebasePlayerSession(
    string name,
    string uuid,
    DateTime lastOnline,
    int timeOnlineSeconds,
    DateTime createdAt)
{
    public DateTime CreatedAt { get; set; } = createdAt;

    public DateTime LastOnline { get; set; } = lastOnline;

    public string Uuid { get; set; } = uuid;

    public string Name { get; set; } = name;

    public int TimeOnlineSeconds { get; set; } = timeOnlineSeconds;
}