using ObservableHelpers;
using RestfulFirebase.Database.Offline;
using RestfulFirebase.Database.Query;
using RestfulFirebase.Utilities;
using RestfulFirebase.Local;
using System;

namespace RestfulFirebase.Database
{
    /// <summary>
    /// App module that provides firebase realtime database implementations.
    /// </summary>
    public class FirebaseDatabaseApp : SyncContext, IAppModule
    {
        #region Properties

        /// <inheritdoc/>
        public RestfulFirebaseApp App { get; private set; }

        /// <summary>
        /// Gets the pending write tasks count on syncer.
        /// </summary>
        public int PendingWrites { get => OfflineDatabase.WriteTaskCount; }

        internal OfflineDatabase OfflineDatabase { get; private set; }

        #endregion

        #region Initializers

        internal FirebaseDatabaseApp(RestfulFirebaseApp app)
        {
            SyncOperation.SetContext(app);

            App = app;
            OfflineDatabase = new OfflineDatabase(app);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Creates new instance of <see cref="ChildQuery"/> node with the specified child <paramref name="resourceName"/>.
        /// </summary>
        /// <param name="resourceName">
        /// The resource name of the node.
        /// </param>
        /// <returns>
        /// The created <see cref="ChildQuery"/> node.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Throws when <paramref name="resourceName"/> is null or empty.
        /// </exception>
        public ChildQuery Child(string resourceName)
        {
            if (string.IsNullOrEmpty(resourceName))
            {
                throw new ArgumentNullException(nameof(resourceName));
            }
            return new ChildQuery(App, () => UrlUtilities.Combine(App.Config.DatabaseURL, resourceName));
        }

        /// <summary>
        /// Flush all data of the offline database.
        /// </summary>
        /// <param name="localDatabase">
        /// Local database to flush. Leave <c>default</c> or <c>null</c> to flush default local database <see cref="FirebaseConfig.LocalDatabase"/>.
        /// </param>
        public void Flush(ILocalDatabase localDatabase = default)
        {
            OfflineDatabase.Flush(localDatabase ?? App.Config.LocalDatabase);
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                OfflineDatabase?.Dispose();
            }
            base.Dispose(disposing);
        }

        #endregion
    }
}
