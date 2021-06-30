using ObservableHelpers;
using RestfulFirebase.Auth;
using RestfulFirebase.Database;
using RestfulFirebase.Database.Offline;
using RestfulFirebase.Local;
using RestfulFirebase.Storage;
using System;
using System.Threading.Tasks;

namespace RestfulFirebase
{
    /// <summary>
    /// App session for whole restful firebase operations.
    /// </summary>
    public class RestfulFirebaseApp : Disposable
    {
        #region Properties

        /// <summary>
        /// Gets <see cref="FirebaseConfig"/> of the app session 
        /// </summary>
        public FirebaseConfig Config { get; }

        /// <summary>
        /// Gets the <see cref="LocalDatabaseApp"/> used for the app persistency.
        /// </summary>
        public LocalDatabaseApp LocalDatabase { get; }

        /// <summary>
        /// Gets the <see cref="FirebaseAuthApp"/> for firebase authentication app module.
        /// </summary>
        public FirebaseAuthApp Auth { get; }

        /// <summary>
        /// Gets the <see cref="FirebaseDatabaseApp"/> for firebase database app module.
        /// </summary>
        public FirebaseDatabaseApp Database { get; }

        /// <summary>
        /// Gets the <see cref="FirebaseStorageApp"/> for firebase storage app module.
        /// </summary>
        public FirebaseStorageApp Storage { get; }

        #endregion

        #region Initializers

        /// <summary>
        /// Creates new instance of <see cref="RestfulFirebaseApp"/> app.
        /// </summary>
        /// <param name="config">
        /// The <see cref="FirebaseConfig"/> configuration used by the app.
        /// </param>
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

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Auth?.Dispose();
                Database?.Dispose();
            }
            base.Dispose(disposing);
        }

        #endregion
    }
}
