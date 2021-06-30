using RestfulFirebase.Auth;
using RestfulFirebase.Database;
using RestfulFirebase.Database.Offline;
using RestfulFirebase.Extensions.Http;
using RestfulFirebase.Local;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace RestfulFirebase
{
    /// <summary>
    /// Provides configuration for <see cref="RestfulFirebaseApp"/> app.
    /// </summary>
    public class FirebaseConfig
    {
        /// <summary>
        /// Creates new instance of <see cref="FirebaseConfig"/> with the default configurations.
        /// </summary>
        public FirebaseConfig()
        {
            LocalDatabase = new DefaultLocalDatabase();
            HttpClientFactory = new DefaultHttpClientFactory();
            HttpStreamFactory = new DefaultHttpStreamFactory();
            AuthRequestTimeout = TimeSpan.FromSeconds(30);
            DatabaseRequestTimeout = TimeSpan.FromSeconds(15);
            DatabaseColdStreamTimeout = TimeSpan.FromMinutes(1);
            DatabaseRetryDelay = TimeSpan.FromSeconds(2);
            StorageRequestTimeout = TimeSpan.FromMinutes(2);
            DatabaseMaxConcurrentWrites = 100;
        }

        /// <summary>
        /// Gets or sets the firebase api key used by the app.
        /// </summary>
        public string ApiKey { get; set; }

        /// <summary>
        /// Gets or sets the firebase database url used by the app.
        /// </summary>
        public string DatabaseURL { get; set; }

        /// <summary>
        /// Gets or sets the firebase storage bucket used by the app.
        /// </summary>
        public string StorageBucket { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="IHttpClientFactory"/> used by the app.
        /// </summary>
        public IHttpClientFactory HttpClientFactory { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="IHttpStreamFactory"/> used by the firebase realtime database streamers.
        /// </summary>
        public IHttpStreamFactory HttpStreamFactory { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="TimeSpan"/> timeout used for all the firebase authentication requests.
        /// </summary>
        public TimeSpan AuthRequestTimeout { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="TimeSpan"/> timeout used for all the firebase realtime database requests.
        /// </summary>
        public TimeSpan DatabaseRequestTimeout { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="TimeSpan"/> timeout used for the firebase realtime database cold streamers.
        /// </summary>
        public TimeSpan DatabaseColdStreamTimeout { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="TimeSpan"/> retry delay for the firebase realtime database failed requests.
        /// </summary>
        public TimeSpan DatabaseRetryDelay { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="TimeSpan"/> timeout for the firebase storage requests.
        /// </summary>
        public TimeSpan StorageRequestTimeout { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="ILocalDatabase"/> used for app persistency and offline database.
        /// </summary>
        public ILocalDatabase LocalDatabase { get; set; }

        /// <summary>
        /// Specify if token returned by factory will be used as "auth" url parameter or "access_token". 
        /// </summary>
        public bool AsAccessToken { get; set; }

        /// <summary>
        /// Gets or sets the network state of the app.
        /// </summary>
        public bool OfflineMode { get; set; }

        /// <summary>
        /// Gets or sets the firebase realtime database max concurrent writes.
        /// </summary>
        public int DatabaseMaxConcurrentWrites { get; set; }
    }
}
