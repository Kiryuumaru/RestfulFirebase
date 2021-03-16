using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace RestfulFirebase.Storage
{
    /// <summary>
    /// Firebase auth app which acts as an entry point to the storage.
    /// </summary>
    public class FirebaseStorageApp : IDisposable
    {
        /// <summary>
        /// Gets the RestfulFirebaseApp
        /// </summary>
        public RestfulFirebaseApp App { get; }

        internal FirebaseStorageApp(RestfulFirebaseApp app)
        {
            App = app;
        }

        /// <summary>
        /// Constructs firebase path to the file.
        /// </summary>
        /// <param name="childRoot"> Root name of the entity. This can be folder or a file name or full path.</param>
        /// <example>
        ///     storage
        ///         .Child("some")
        ///         .Child("path")
        ///         .Child("to/file.png");
        /// </example>
        /// <returns> <see cref="FirebaseStorageReference"/> for fluid syntax. </returns>
        public FirebaseStorageReference Child(string childRoot)
        {
            return new FirebaseStorageReference(App, childRoot);
        }

        /// <summary>
        /// Creates a new <see cref="HttpClient"/> with authentication header when <see cref="RestfulFirebaseApp.Auth"/> is authenticated.
        /// </summary>
        /// <param name="timeout">Request timeout.</param>
        /// <returns> The authenticated <see cref="HttpClient"/>. </returns>
        public HttpClient CreateHttpClientAsync(TimeSpan? timeout = null)
        {
            var client = App.Config.HttpClientFactory.GetHttpClient(timeout).GetHttpClient();

            if (App.Auth.Authenticated)
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Firebase", App.Auth.FirebaseToken);
            }

            return client;
        }

        public void Dispose()
        {

        }
    }
}
