using ObservableHelpers;
using RestfulFirebase.Database.Offline;
using RestfulFirebase.Database.Query;
using RestfulFirebase.Database.Streaming;
using RestfulFirebase.Utilities;
using RestfulFirebase.Http;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

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
        public ChildQuery Child(string resourceName)
        {
            return new ChildQuery(App, () => UrlUtilities.Combine(App.Config.DatabaseURL, resourceName));
        }

        /// <summary>
        /// Flush all data of the offline database.
        /// </summary>
        public void Flush()
        {
            OfflineDatabase.Flush();
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
