using ObservableHelpers;
using RestfulFirebase.Database.Query;
using RestfulFirebase.Utilities;
using RestfulFirebase.Local;
using System;
using ObservableHelpers.Utilities;

namespace RestfulFirebase.Database
{
    /// <summary>
    /// App module that provides firebase realtime database implementations.
    /// </summary>
    public class DatabaseApp : SyncContext, IAppModule
    {
        #region Properties

        /// <inheritdoc/>
        public RestfulFirebaseApp App { get; private set; }

        internal const string OfflineDatabaseIndicator = "db";

        #endregion

        #region Initializers

        internal DatabaseApp(RestfulFirebaseApp app)
        {
            SyncOperation.SetContext(app);

            App = app;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Creates new instance of <see cref="ChildQuery"/> node with the specified child <paramref name="resourceName"/>.
        /// </summary>
        /// <param name="resourceNameFactory">
        /// The resource name factory of the node.
        /// </param>
        /// <returns>
        /// The created <see cref="ChildQuery"/> node.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Throws when <paramref name="resourceNameFactory"/> is null.
        /// </exception>
        public ChildQuery Child(Func<string> resourceNameFactory)
        {
            if (resourceNameFactory == null)
            {
                throw new ArgumentNullException(nameof(resourceNameFactory));
            }
            return new ChildQuery(App, null, () => UrlUtilities.Combine(App.Config.DatabaseURL, resourceNameFactory()));
        }

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
            return Child(() => resourceName);
        }

        /// <summary>
        /// Flush all data of the offline database.
        /// </summary>
        /// <param name="localDatabase">
        /// Local database to flush. Leave <c>default</c> or <c>null</c> to flush default local database <see cref="FirebaseConfig.LocalDatabase"/>.
        /// </param>
        public void Flush(ILocalDatabase localDatabase = default)
        {
            App.LocalDatabase.InternalDelete(localDatabase, new string[] { OfflineDatabaseIndicator });
        }

        #endregion
    }
}
