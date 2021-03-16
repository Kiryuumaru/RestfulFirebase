using RestfulFirebase.Database.Query;
using RestfulFirebase.Extensions.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace RestfulFirebase.Database
{
    /// <summary>
    /// Firebase auth app which acts as an entry point to the database.
    /// </summary>
    public class FirebaseDatabaseApp : IDisposable
    {
        /// <summary>
        /// Gets the RestfulFirebaseApp
        /// </summary>
        public RestfulFirebaseApp App { get; }

        internal FirebaseDatabaseApp(RestfulFirebaseApp app)
        {
            App = app;
        }

        /// <summary>
        /// Queries for a child of the data root.
        /// </summary>
        /// <param name="resourceName"> Name of the child. </param>
        /// <returns> <see cref="ChildQuery"/>. </returns>
        public ChildQuery Child(string resourceName)
        {
            return new ChildQuery(App, () => App.Config.DatabaseURL + resourceName);
        }

        public void Dispose()
        {

        }
    }
}
