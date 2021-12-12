using ObservableHelpers;
using RestfulFirebase.Http;
using RestfulFirebase.Local;
using System;
using System.Collections.Generic;

namespace RestfulFirebase
{
    /// <summary>
    /// Provides configuration for <see cref="RestfulFirebaseApp"/> app.
    /// </summary>
    public class FirebaseConfig : ObservableObject
    {
        #region Properties

        /// <summary>
        /// Gets or sets the firebase api key used by the app.
        /// </summary>
        public string ApiKey
        {
            get => GetProperty<string>();
            set => SetProperty(value);
        }

        /// <summary>
        /// Gets or sets the firebase database url used by the app.
        /// </summary>
        public string DatabaseURL
        {
            get => GetProperty<string>();
            set => SetProperty(value, postAction: args =>
            {
                if (!args.newValue.EndsWith("/"))
                {
                    DatabaseURL += "/";
                }
            });
        }

        /// <summary>
        /// Gets or sets the firebase storage bucket used by the app.
        /// </summary>
        public string StorageBucket
        {
            get => GetProperty<string>();
            set => SetProperty(value);
        }

        /// <summary>
        /// Gets or sets the <see cref="IHttpClientFactory"/> used by the app.
        /// </summary>
        public IHttpClientFactory HttpClientFactory
        {
            get => GetProperty<IHttpClientFactory>(new StockHttpClientFactory());
            set => SetProperty(value);
        }

        /// <summary>
        /// Gets or sets the <see cref="IHttpStreamFactory"/> used by the firebase realtime database streamers.
        /// </summary>
        public IHttpStreamFactory HttpStreamFactory
        {
            get => GetProperty<IHttpStreamFactory>(new StockHttpStreamFactory());
            set => SetProperty(value);
        }

        /// <summary>
        /// Gets or sets the <see cref="TimeSpan"/> timeout used for all the firebase authentication requests.
        /// </summary>
        public TimeSpan AuthRequestTimeout
        {
            get => GetProperty<TimeSpan>(TimeSpan.FromSeconds(30));
            set => SetProperty(value);
        }

        /// <summary>
        /// Gets or sets the <see cref="TimeSpan"/> timeout used for all the firebase realtime database requests.
        /// </summary>
        public TimeSpan DatabaseRequestTimeout
        {
            get => GetProperty<TimeSpan>(TimeSpan.FromSeconds(15));
            set => SetProperty(value);
        }

        /// <summary>
        /// Gets or sets the <see cref="TimeSpan"/> timeout used for the firebase realtime database unresponsive streamers.
        /// </summary>
        public TimeSpan DatabaseColdStreamTimeout
        {
            get => GetProperty<TimeSpan>(TimeSpan.FromMinutes(1));
            set => SetProperty(value);
        }

        /// <summary>
        /// Gets or sets the <see cref="TimeSpan"/> retry delay for the firebase realtime database failed requests.
        /// </summary>
        public TimeSpan DatabaseRetryDelay
        {
            get => GetProperty<TimeSpan>(TimeSpan.FromSeconds(2));
            set => SetProperty(value);
        }

        /// <summary>
        /// Gets or sets the <see cref="TimeSpan"/> timeout for the firebase storage requests.
        /// </summary>
        public TimeSpan StorageRequestTimeout
        {
            get => GetProperty<TimeSpan>(TimeSpan.FromMinutes(2));
            set => SetProperty(value);
        }

        /// <summary>
        /// Gets or sets the <see cref="ILocalDatabase"/> used for auth persistency and offline database.
        /// </summary>
        public ILocalDatabase LocalDatabase
        {
            get => GetProperty<ILocalDatabase>(new StockLocalDatabase());
            set => SetProperty(value);
        }

        /// <summary>
        /// Gets or sets the <see cref="ILocalDatabase"/> to optionally separate the auth persistency storage from the <see cref="LocalDatabase"/>.
        /// </summary>
        public ILocalDatabase CustomAuthLocalDatabase
        {
            get => GetProperty<ILocalDatabase>();
            set => SetProperty(value);
        }

        /// <summary>
        /// Gets or sets the <see cref="ILocalEncryption"/> used for local database security.
        /// </summary>
        public ILocalEncryption LocalEncryption
        {
            get => GetProperty<ILocalEncryption>(new StockLocalNonEncryption());
            set => SetProperty(value);
        }

        /// <summary>
        /// Specify if token returned by factory will be used as "auth" url parameter or "access_token". 
        /// </summary>
        public bool AsAccessToken
        {
            get => GetProperty<bool>();
            set => SetProperty(value);
        }

        /// <summary>
        /// Gets or sets the network state of the app.
        /// </summary>
        public bool OfflineMode
        {
            get => GetProperty<bool>();
            set => SetProperty(value);
        }

        /// <summary>
        /// Gets or sets whether the <see cref="IEnumerable{T}"/> types will be serialized as blobs.
        /// </summary>
        public bool DatabaseSerializeEnumerableAsBlobs
        {
            get => GetProperty<bool>();
            set => SetProperty(value);
        }

        /// <summary>
        /// Gets or sets the firebase realtime database max concurrent writes.
        /// </summary>
        public int DatabaseMaxConcurrentSyncWrites
        {
            get => GetProperty<int>(100);
            set => SetProperty(value);
        }

        #endregion

        #region Initializers

        /// <summary>
        /// Creates new instance of <see cref="FirebaseConfig"/> with the default configurations.
        /// </summary>
        public FirebaseConfig()
        {

        }

        #endregion

        #region Methods



        #endregion
    }
}
