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
        private readonly string baseUrl;

        internal readonly IHttpClientProxy HttpClient;

        /// <summary>
        /// Gets the RestfulFirebaseApp
        /// </summary>
        public RestfulFirebaseApp App { get; }

        internal FirebaseDatabaseApp(RestfulFirebaseApp app)
        {
            App = app;

            HttpClient = App.Config.HttpClientFactory.GetHttpClient(null);

            if (!baseUrl.EndsWith("/"))
            {
                baseUrl += "/";
            }
        }

        /// <summary>
        /// Queries for a child of the data root.
        /// </summary>
        /// <param name="resourceName"> Name of the child. </param>
        /// <returns> <see cref="ChildQuery"/>. </returns>
        public ChildQuery Child(string resourceName)
        {
            return new ChildQuery(App, () => baseUrl + resourceName);
        }

        public void Dispose()
        {
            HttpClient?.Dispose();
        }
    }
}
