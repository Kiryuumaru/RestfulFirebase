using ObservableHelpers;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace RestfulFirebase.Storage
{
    /// <summary>
    /// App module that provides firebase storage implementations
    /// </summary>
    public class FirebaseStorageApp : Disposable, IAppModule
    {
        #region Properties

        /// <inheritdoc/>
        public RestfulFirebaseApp App { get; }

        #endregion

        #region Initializers

        internal FirebaseStorageApp(RestfulFirebaseApp app)
        {
            App = app;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Creates new instance of <see cref="FirebaseStorageReference"/> child reference.
        /// </summary>
        /// <param name="childRoot">
        /// The child reference name or file name.
        /// </param>
        /// <returns>
        /// The instance of <see cref="FirebaseStorageReference"/> child reference.
        /// </returns>
        public FirebaseStorageReference Child(string childRoot)
        {
            return new FirebaseStorageReference(App, childRoot);
        }

        internal HttpClient CreateHttpClientAsync(TimeSpan? timeout = null)
        {
            var client = App.Config.HttpClientFactory.GetHttpClient(timeout).GetHttpClient();

            if (App.Auth.IsAuthenticated)
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Firebase", App.Auth.Session.FirebaseToken);
            }

            return client;
        }

        #endregion
    }
}
