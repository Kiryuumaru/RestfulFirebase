using ObservableHelpers;
using RestfulFirebase.Http;
using RestfulFirebase.Local;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace RestfulFirebase;

/// <summary>
/// Provides configuration for <see cref="RestfulFirebaseApp"/> app.
/// </summary>
public class FirebaseConfig : ObservableObject
{
    #region Properties

    /// <summary>
    /// Gets or sets the firebase API key.
    /// </summary>
    public string ApiKey { get; }

    /// <summary>
    /// Gets or sets the firebase API key.
    /// </summary>
    public string ProjectId { get; }

    /// <summary>
    /// Gets or sets the <see cref="ILocalDatabase"/> used for auth persistency and offline database.
    /// </summary>
    public ILocalDatabase LocalDatabase
    {
        get => GetProperty<ILocalDatabase>(() => new StockLocalDatabase());
        set => SetProperty(value);
    }

    /// <summary>
    /// Gets or sets the <see cref="ILocalDatabase"/> to optionally separate the auth persistency storage from the <see cref="LocalDatabase"/>.
    /// </summary>
    public ILocalDatabase? CustomAuthLocalDatabase
    {
        get => GetProperty<ILocalDatabase?>();
        set => SetProperty(value);
    }

    /// <summary>
    /// Gets or sets the <see cref="ILocalEncryption"/> used for local database security.
    /// </summary>
    public ILocalEncryption? LocalEncryption
    {
        get => GetProperty<ILocalEncryption?>();
        set => SetProperty(value);
    }

    /// <summary>
    /// Gets or sets the <see cref="IHttpClientFactory"/>.
    /// </summary>
    public IHttpClientFactory HttpClientFactory
    {
        get => GetProperty<IHttpClientFactory>(() => new StockHttpClientFactory());
        set => SetProperty(value);
    }

    /// <summary>
    /// Gets or sets the <see cref="IHttpStreamFactory"/> used by the firebase realtime database streamers.
    /// </summary>
    public IHttpStreamFactory HttpStreamFactory
    {
        get => GetProperty<IHttpStreamFactory>(() => new StockHttpStreamFactory());
        set => SetProperty(value);
    }

    /// <summary>
    /// Gets or sets the <see cref="TimeSpan"/> timeout used for all the firebase authentication requests.
    /// </summary>
    public TimeSpan AuthRequestTimeout
    {
        get => GetProperty(() => TimeSpan.FromSeconds(30));
        set => SetProperty(value);
    }

    /// <summary>
    /// Gets or sets the <see cref="TimeSpan"/> timeout used for all the firebase realtime database requests.
    /// </summary>
    public TimeSpan DatabaseRequestTimeout
    {
        get => GetProperty(() => TimeSpan.FromSeconds(15));
        set => SetProperty(value);
    }

    /// <summary>
    /// Gets or sets the <see cref="TimeSpan"/> timeout used for the firebase realtime database unresponsive streamers.
    /// </summary>
    public TimeSpan DatabaseColdStreamTimeout
    {
        get => GetProperty(() => TimeSpan.FromMinutes(1));
        set => SetProperty(value);
    }

    /// <summary>
    /// Gets or sets the <see cref="TimeSpan"/> retry delay for the firebase realtime database failed requests.
    /// </summary>
    public TimeSpan DatabaseRetryDelay
    {
        get => GetProperty(() => TimeSpan.FromSeconds(2));
        set => SetProperty(value);
    }

    /// <summary>
    /// Gets or sets the <see cref="TimeSpan"/> timeout for the firebase storage requests.
    /// </summary>
    public TimeSpan StorageRequestTimeout
    {
        get => GetProperty(() => TimeSpan.FromMinutes(2));
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
        get => GetProperty(() => 100);
        set => SetProperty(value);
    }

    internal ILocalDatabase CachedLocalDatabase
    {
        get
        {
            if (cachedLocalDatabase == null)
            {
                cachedLocalDatabase = LocalDatabase;
            }
            return cachedLocalDatabase;
        }
    }

    internal ILocalDatabase? CachedCustomAuthLocalDatabase { get; private set; }

    internal ILocalEncryption? CachedLocalEncryption { get; private set; }

    internal IHttpClientFactory CachedHttpClientFactory
    {
        get
        {
            if (cachedHttpClientFactory == null)
            {
                cachedHttpClientFactory = HttpClientFactory;
            }
            return cachedHttpClientFactory;
        }
    }

    internal IHttpStreamFactory CachedHttpStreamFactory
    {
        get
        {
            if (cachedHttpStreamFactory == null)
            {
                cachedHttpStreamFactory = HttpStreamFactory;
            }
            return cachedHttpStreamFactory;
        }
    }

    internal TimeSpan CachedAuthRequestTimeout { get; private set; }

    internal TimeSpan CachedDatabaseRequestTimeout { get; private set; }

    internal TimeSpan CachedDatabaseColdStreamTimeout { get; private set; }

    internal TimeSpan CachedDatabaseRetryDelay { get; private set; }

    internal TimeSpan CachedStorageRequestTimeout { get; private set; }

    internal bool CachedAsAccessToken { get; private set; }

    internal bool CachedOfflineMode { get; private set; }

    internal bool CachedDatabaseSerializeEnumerableAsBlobs { get; private set; }

    internal int CachedDatabaseMaxConcurrentSyncWrites { get; private set; }

    private ILocalDatabase? cachedLocalDatabase;

    private IHttpClientFactory? cachedHttpClientFactory;

    private IHttpStreamFactory? cachedHttpStreamFactory;

    #endregion

    #region Initializers

    /// <summary>
    /// Creates new instance of <see cref="FirebaseConfig"/> with the default configurations.
    /// </summary>
    /// <param name="apiKey">
    /// The API key of the app.
    /// </param>
    /// <param name="projectId">
    /// The project ID of the app.
    /// </param>
    public FirebaseConfig(string projectId, string apiKey)
    {
        ApiKey = apiKey;
        ProjectId = projectId;

        AttachOnPropertyChanged<ILocalDatabase>(v => cachedLocalDatabase = v, nameof(LocalDatabase));
        AttachOnPropertyChanged<ILocalDatabase>(v => CachedCustomAuthLocalDatabase = v, nameof(CustomAuthLocalDatabase));
        AttachOnPropertyChanged<ILocalEncryption>(v => CachedLocalEncryption = v, nameof(LocalEncryption));
        AttachOnPropertyChanged<IHttpClientFactory>(v => cachedHttpClientFactory = v, nameof(HttpClientFactory));
        AttachOnPropertyChanged<IHttpStreamFactory>(v => cachedHttpStreamFactory = v, nameof(HttpStreamFactory));
        AttachOnPropertyChanged<TimeSpan>(v => CachedAuthRequestTimeout = v, nameof(AuthRequestTimeout));
        AttachOnPropertyChanged<TimeSpan>(v => CachedDatabaseRequestTimeout = v, nameof(DatabaseRequestTimeout));
        AttachOnPropertyChanged<TimeSpan>(v => CachedDatabaseColdStreamTimeout = v, nameof(DatabaseColdStreamTimeout));
        AttachOnPropertyChanged<TimeSpan>(v => CachedDatabaseRetryDelay = v, nameof(DatabaseRetryDelay));
        AttachOnPropertyChanged<TimeSpan>(v => CachedStorageRequestTimeout = v, nameof(StorageRequestTimeout));
        AttachOnPropertyChanged<bool>(v => CachedAsAccessToken = v, nameof(AsAccessToken));
        AttachOnPropertyChanged<bool>(v => CachedOfflineMode = v, nameof(OfflineMode));
        AttachOnPropertyChanged<bool>(v => CachedDatabaseSerializeEnumerableAsBlobs = v, nameof(DatabaseSerializeEnumerableAsBlobs));
        AttachOnPropertyChanged<int>(v => CachedDatabaseMaxConcurrentSyncWrites = v, nameof(DatabaseMaxConcurrentSyncWrites));

        InitializeProperties();
    }

    #endregion

    #region Methods



    #endregion
}
