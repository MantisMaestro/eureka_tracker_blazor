using Client.FirebaseMigration.Models;
using Google.Cloud.Firestore;

namespace Client.FirebaseMigration;

public class FirebaseProvider
{
    private readonly FirestoreDb _db = FirestoreDb.Create("eurekaonline-bdcf2");

    public async Task ListAvailableCollections()
    {
        var collections = await _db.ListRootCollectionsAsync().ToListAsync();
        foreach (var collection in collections) Console.WriteLine(collection.Id);
    }

    public async Task<FirebaseModel> BuildFirebaseModel()
    {
        var model = new FirebaseModel();

        var dayRecords = await _db.ListRootCollectionsAsync().ToListAsync();
        foreach (var dayRecord in dayRecords)
        {
            Console.WriteLine($"Getting data for: {dayRecord.Id}");
            if (dayRecord.Id is "online_now" or "players") continue;

            var dayRecordModel = new FirebaseDayRecord { Date = dayRecord.Id };
            var playerSessions = await dayRecord.ListDocumentsAsync().ToListAsync();

            foreach (var playerSession in playerSessions)
            {
                var playerSessionData = await playerSession.GetSnapshotAsync();

                playerSessionData.TryGetValue("name", out string name);
                playerSessionData.TryGetValue("last_online", out DateTime lastOnline);
                playerSessionData.TryGetValue("time_online_seconds", out int timeOnlineSeconds);

                var playerSessionModel = new FirebasePlayerSession(
                    name,
                    playerSessionData.Id,
                    lastOnline,
                    timeOnlineSeconds,
                    DateTime.Parse(dayRecord.Id)
                );

                dayRecordModel.PlayerSessions.Add(playerSessionModel);
            }

            model.DayRecords.Add(dayRecordModel);
        }

        return model;
    }
}