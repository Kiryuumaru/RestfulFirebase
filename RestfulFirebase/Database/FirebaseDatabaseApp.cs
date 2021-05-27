using RestfulFirebase.Database.Offline;
using RestfulFirebase.Database.Query;
using RestfulFirebase.Database.Streaming;
using RestfulFirebase.Extensions.Http;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace RestfulFirebase.Database
{
    public class FirebaseDatabaseApp : IDisposable
    {
        #region Properties

        public RestfulFirebaseApp App { get; private set; }
        internal OfflineDatabase OfflineDatabase { get; private set; }

        #endregion

        #region Initializers

        internal FirebaseDatabaseApp(RestfulFirebaseApp app)
        {
            App = app;
            OfflineDatabase = new OfflineDatabase(app);
        }

        #endregion

        #region Methods

        public ChildQuery Child(string resourceName)
        {
            return new ChildQuery(App, () => Utils.UrlCombine(App.Config.DatabaseURL, resourceName));
        }

        public void Flush()
        {
            OfflineDatabase.Flush();
        }

        public void Dispose()
        {
            OfflineDatabase?.Dispose();
        }

        #endregion
    }
}
