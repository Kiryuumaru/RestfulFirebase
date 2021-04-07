using RestfulFirebase.Auth;
using RestfulFirebase.Database;
using RestfulFirebase.Database.Offline;
using RestfulFirebase.Local;
using RestfulFirebase.Storage;
using System;

namespace RestfulFirebase
{
    public class RestfulFirebaseApp : IDisposable
    {
        public FirebaseConfig Config { get; }

        public LocalDatabaseApp LocalDatabase { get; }

        public FirebaseAuthApp Auth { get; }

        public FirebaseDatabaseApp Database { get; }

        public FirebaseStorageApp Storage { get; }

        public RestfulFirebaseApp(FirebaseConfig config)
        {
            Config = config;

            if (!Config.DatabaseURL.EndsWith("/"))
            {
                Config.DatabaseURL += "/";
            }

            LocalDatabase = new LocalDatabaseApp(this);
            Auth = new FirebaseAuthApp(this);
            Database = new FirebaseDatabaseApp(this);
            Storage = new FirebaseStorageApp(this);
        }

        public void Dispose()
        {
            Auth?.Dispose();
            Database?.Dispose();
        }
    }
}
