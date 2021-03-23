using RestfulFirebase.Database.Offline;
using RestfulFirebase.Database.Query;
using RestfulFirebase.Extensions.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace RestfulFirebase.Database
{
    public class FirebaseDatabaseApp : IDisposable
    {
        public RestfulFirebaseApp App { get; }
        public OfflineActionStore OfflinePersistence { get; }

        internal FirebaseDatabaseApp(RestfulFirebaseApp app)
        {
            App = app;
            OfflinePersistence = new OfflineActionStore(app);
        }

        public ChildQuery Child(string resourceName)
        {
            return new ChildQuery(() => App.Config.DatabaseURL + resourceName, App);
        }

        public void Dispose()
        {

        }
    }
}
