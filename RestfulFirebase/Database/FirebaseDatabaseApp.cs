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

        internal FirebaseDatabaseApp(RestfulFirebaseApp app)
        {
            App = app;
        }

        public ChildQuery Child(string resourceName)
        {
            return new ChildQuery(App, () => App.Config.DatabaseURL + resourceName);
        }

        public void Dispose()
        {

        }
    }
}
