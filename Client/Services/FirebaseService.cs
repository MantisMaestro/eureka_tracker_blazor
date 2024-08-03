using Client.FirebaseMigration;
using Client.FirebaseMigration.Models;
using EurekaDb.Context;

namespace Client.Services;

public class FirebaseService(EurekaContext context)
{
    public EurekaContext Context { get; } = context;

    public static async Task<FirebaseModel?> BuildFirebaseModel()
    {
        var migrateFirebase = new FirebaseProvider();
        var model = await migrateFirebase.BuildFirebaseModel();

        return model;
    }
}