using RestfulFirebase.Auth;
using RestfulFirebase.Database;
using RestfulFirebase.Database.Offline;
using RestfulFirebase.Local;
using RestfulFirebase.Storage;
using System;
using System.Threading.Tasks;

namespace RestfulFirebase
{
    public class RestfulFirebaseApp : IDisposable
    {
        #region Properties

        public FirebaseConfig Config { get; }

        public LocalDatabaseApp LocalDatabase { get; }

        public FirebaseAuthApp Auth { get; }

        public FirebaseDatabaseApp Database { get; }

        public FirebaseStorageApp Storage { get; }

        #endregion

        #region Initializers

        public RestfulFirebaseApp(FirebaseConfig config)
        {
            Config = config;

            if (!Config.DatabaseURL.EndsWith("/"))
            {
                Config.DatabaseURL += "/";
            }

            LocalDatabase = new LocalDatabaseApp(this);
            Database = new FirebaseDatabaseApp(this);
            Storage = new FirebaseStorageApp(this);
            Auth = new FirebaseAuthApp(this);
        }

        #endregion

        #region Methods

        public void Dispose()
        {
            Auth?.Dispose();
            Database?.Dispose();
        }

        #endregion
    }
}
