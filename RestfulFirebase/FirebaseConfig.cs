using ObservableHelpers;
using RestfulFirebase.Http;
using RestfulFirebase.Local;
using System;
using System.Collections.Generic;
using System.ComponentModel;

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

        internal string CachedApiKey { get; private set; }

        internal string CachedDatabaseURL { get; private set; }

        internal string CachedStorageBucket { get; private set; }

        internal IHttpClientFactory CachedHttpClientFactory { get; private set; }

        internal IHttpStreamFactory CachedHttpStreamFactory { get; private set; }

        internal TimeSpan CachedAuthRequestTimeout { get; private set; }

        internal TimeSpan CachedDatabaseRequestTimeout { get; private set; }

        internal TimeSpan CachedDatabaseColdStreamTimeout { get; private set; }

        internal TimeSpan CachedDatabaseRetryDelay { get; private set; }

        internal TimeSpan CachedStorageRequestTimeout { get; private set; }

        internal ILocalDatabase CachedLocalDatabase { get; private set; }

        internal ILocalDatabase CachedCustomAuthLocalDatabase { get; private set; }

        internal ILocalEncryption CachedLocalEncryption { get; private set; }

        internal bool CachedAsAccessToken { get; private set; }

        internal bool CachedOfflineMode { get; private set; }

        internal bool CachedDatabaseSerializeEnumerableAsBlobs { get; private set; }

        internal int CachedDatabaseMaxConcurrentSyncWrites { get; private set; }

        #endregion

        #region Initializers

        /// <summary>
        /// Creates new instance of <see cref="FirebaseConfig"/> with the default configurations.
        /// </summary>
        public FirebaseConfig()
        {
            AttachOnImmediatePropertyChanged<string>(v => CachedApiKey = v, nameof(ApiKey));
            AttachOnImmediatePropertyChanged<string>(v => CachedDatabaseURL = v, nameof(DatabaseURL));
            AttachOnImmediatePropertyChanged<string>(v => CachedStorageBucket = v, nameof(StorageBucket));
            AttachOnImmediatePropertyChanged<IHttpClientFactory>(v => CachedHttpClientFactory = v, nameof(HttpClientFactory));
            AttachOnImmediatePropertyChanged<IHttpStreamFactory>(v => CachedHttpStreamFactory = v, nameof(HttpStreamFactory));
            AttachOnImmediatePropertyChanged<TimeSpan>(v => CachedAuthRequestTimeout = v, nameof(AuthRequestTimeout));
            AttachOnImmediatePropertyChanged<TimeSpan>(v => CachedDatabaseRequestTimeout = v, nameof(DatabaseRequestTimeout));
            AttachOnImmediatePropertyChanged<TimeSpan>(v => CachedDatabaseColdStreamTimeout = v, nameof(DatabaseColdStreamTimeout));
            AttachOnImmediatePropertyChanged<TimeSpan>(v => CachedDatabaseRetryDelay = v, nameof(DatabaseRetryDelay));
            AttachOnImmediatePropertyChanged<TimeSpan>(v => CachedStorageRequestTimeout = v, nameof(StorageRequestTimeout));
            AttachOnImmediatePropertyChanged<ILocalDatabase>(v => CachedLocalDatabase = v, nameof(LocalDatabase));
            AttachOnImmediatePropertyChanged<ILocalDatabase>(v => CachedCustomAuthLocalDatabase = v, nameof(CustomAuthLocalDatabase));
            AttachOnImmediatePropertyChanged<ILocalEncryption>(v => CachedLocalEncryption = v, nameof(LocalEncryption));
            AttachOnImmediatePropertyChanged<bool>(v => CachedAsAccessToken = v, nameof(AsAccessToken));
            AttachOnImmediatePropertyChanged<bool>(v => CachedOfflineMode = v, nameof(OfflineMode));
            AttachOnImmediatePropertyChanged<bool>(v => CachedDatabaseSerializeEnumerableAsBlobs = v, nameof(DatabaseSerializeEnumerableAsBlobs));
            AttachOnImmediatePropertyChanged<int>(v => CachedDatabaseMaxConcurrentSyncWrites = v, nameof(DatabaseMaxConcurrentSyncWrites));
        }

        #endregion

        #region Methods



        #endregion
    }
}
