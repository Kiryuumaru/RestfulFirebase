using RestfulFirebase.Auth;
using RestfulFirebase.Database;
using RestfulFirebase.Storage;
using System;

namespace RestfulFirebase
{
    /// <summary>
    /// Firebase App which acts as an entry point to the entire rest api calls.
    /// </summary>
    public class RestfulFirebaseApp : IDisposable
    {
        /// <summary>
        /// Gets the firebase app configs.
        /// </summary>
        public FirebaseConfig Config { get; }

        /// <summary>
        /// Gets the app authentication.
        /// </summary>
        public FirebaseAuthApp Auth { get; }

        /// <summary>
        /// Gets the app database entry point.
        /// </summary>
        public FirebaseDatabaseApp Database { get; }

        /// <summary>
        /// Gets the app storage entry point.
        /// </summary>
        public FirebaseStorageApp Storage { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RestfulFirebaseApp"/> class.
        /// </summary>
        /// <param name="config"> The app config. </param>
        public RestfulFirebaseApp(FirebaseConfig config)
        {
            Config = config;

            if (!Config.DatabaseURL.EndsWith("/"))
            {
                Config.DatabaseURL += "/";
            }

            Auth = new FirebaseAuthApp(this);
            Database = new FirebaseDatabaseApp(this);
            Storage = new FirebaseStorageApp(this);
        }

        /// <summary>
        /// Disposes this instance.  
        /// </summary>
        public void Dispose()
        {
            Auth?.Dispose();
            Database?.Dispose();
        }
    }
}
