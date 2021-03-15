using Newtonsoft.Json;
using RestfulFirebase.Database.Offline;
using RestfulFirebase.Extensions.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace RestfulFirebase
{
    /// <summary>
    /// The auth config. 
    /// </summary>
    public class FirebaseConfig
    {
        public FirebaseConfig()
        {
            OfflineDatabaseFactory = (t, s) => new Dictionary<string, OfflineEntry>();
            SubscriptionStreamReaderFactory = s => new StreamReader(s);
            JsonSerializerSettings = new JsonSerializerSettings();
            SyncPeriod = TimeSpan.FromSeconds(10);
            HttpClientFactory = new TransientHttpClientFactory();
        }

        /// <summary>
        /// Gets or sets the api key of your Firebase app.
        /// </summary>
        public string ApiKey
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the database URL of your Firebase app.
        /// </summary>
        public string DatabaseURL
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the storage bucket of your Firebase app.
        /// </summary>
        public string StorageBucket
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the factory for Firebase offline database. Default is in-memory dictionary.
        /// </summary>
        public Func<Type, string, IDictionary<string, OfflineEntry>> OfflineDatabaseFactory
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the factory for <see cref="TextReader"/> used for reading online streams. Default is <see cref="StreamReader"/>.
        /// </summary>
        public Func<Stream, TextReader> SubscriptionStreamReaderFactory
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the json serializer settings.
        /// </summary>
        public JsonSerializerSettings JsonSerializerSettings
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the time between synchronization attempts for pulling and pushing offline entities. Default is 10 seconds.
        /// </summary>
        public TimeSpan SyncPeriod
        {
            get;
            set;
        }

        /// <summary>
        /// Specify if token returned by factory will be used as "auth" url parameter or "access_token". 
        /// </summary>
        public bool AsAccessToken
        {
            get;
            set;
        }

        /// <summary>
        /// Specify HttpClient factory to manage <see cref="System.Net.Http.HttpClient" /> lifecycle.
        /// </summary>
        public IHttpClientFactory HttpClientFactory
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets whether <see cref="TaskCanceledException"/> should be thrown when cancelling a running <see cref="FirebaseStorageTask"/>.
        /// </summary>
        public bool StorageThrowOnCancel
        {
            get;
            set;
        }

        /// <summary>
        /// Timeout of the <see cref="HttpClient"/>. Default is 100s.
        /// </summary>
        public TimeSpan HttpClientTimeout
        {
            get;
            set;
        }

        /// <summary>
        /// Creates a new <see cref="HttpClient"/> with authentication header when <see cref="FirebaseStorageOptions.AuthTokenAsyncFactory"/> is specified.
        /// </summary>
        /// <param name="options">Firebase storage options.</param>
        public async Task<HttpClient> CreateHttpClientAsync()
        {
            var client = new HttpClient();

            if (HttpClientTimeout != default)
            {
                client.Timeout = HttpClientTimeout;
            }

            if (AuthTokenAsyncFactory != null)
            {
                var auth = await AuthTokenAsyncFactory().ConfigureAwait(false);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Firebase", auth);
            }

            return client;


            if (client == null)
            {
                client = App.Config.HttpClientFactory.GetHttpClient(timeout ?? DEFAULT_HTTP_CLIENT_TIMEOUT);
            }

            return client.GetHttpClient();
        }
    }
}
