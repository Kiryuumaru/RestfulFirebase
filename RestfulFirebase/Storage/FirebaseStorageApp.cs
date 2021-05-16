using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace RestfulFirebase.Storage
{
    public class FirebaseStorageApp : IDisposable
    {
        public RestfulFirebaseApp App { get; }

        internal FirebaseStorageApp(RestfulFirebaseApp app)
        {
            App = app;
        }

        public FirebaseStorageReference Child(string childRoot)
        {
            return new FirebaseStorageReference(App, childRoot);
        }

        public HttpClient CreateHttpClientAsync(TimeSpan? timeout = null)
        {
            var client = App.Config.HttpClientFactory.GetHttpClient(timeout).GetHttpClient();

            if (App.Auth.Authenticated)
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Firebase", App.Auth.Session.FirebaseToken);
            }

            return client;
        }

        public void Dispose()
        {

        }
    }
}
