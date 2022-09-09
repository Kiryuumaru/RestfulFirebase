using ObservableHelpers.ComponentModel;
using RestfulFirebase.Http;
using RestfulFirebase.Local;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text.Json;

namespace RestfulFirebase;

/// <summary>
/// Provides configuration for <see cref="RestfulFirebaseApp"/> app.
/// </summary>
[ObservableObject]
public partial class FirebaseConfig
{
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
    [ObservableProperty]
    private ILocalDatabase localDatabase = new StockLocalDatabase();

    /// <summary>
    /// Gets or sets the <see cref="ILocalEncryption"/> used for local database security.
    /// </summary>
    [ObservableProperty]
    private ILocalEncryption? localEncryption;

    /// <summary>
    /// Gets or sets the <see cref="IHttpClientFactory"/>.
    /// </summary>
    [ObservableProperty]
    private IHttpClientFactory httpClientFactory = new StockHttpClientFactory();

    /// <summary>
    /// Gets or sets the <see cref="IHttpStreamFactory"/> used by the firebase realtime database streamers.
    /// </summary>
    [ObservableProperty]
    private IHttpStreamFactory httpStreamFactory = new StockHttpStreamFactory();

    /// <summary>
    /// Gets or sets the <see cref="TimeSpan"/> timeout used for all the firebase authentication requests.
    /// </summary>
    [ObservableProperty]
    private TimeSpan authRequestTimeout = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Gets or sets the <see cref="TimeSpan"/> timeout used for all the firebase realtime database requests.
    /// </summary>
    [ObservableProperty]
    private TimeSpan databaseRequestTimeout = TimeSpan.FromSeconds(15);

    /// <summary>
    /// Gets or sets the <see cref="TimeSpan"/> timeout used for the firebase realtime database unresponsive streamers.
    /// </summary>
    [ObservableProperty]
    private TimeSpan databaseColdStreamTimeout = TimeSpan.FromMinutes(1);

    /// <summary>
    /// Gets or sets the <see cref="TimeSpan"/> retry delay for the firebase realtime database failed requests.
    /// </summary>
    [ObservableProperty]
    private TimeSpan databaseRetryDelay = TimeSpan.FromSeconds(2);

    /// <summary>
    /// Gets or sets the <see cref="TimeSpan"/> timeout for the firebase storage requests.
    /// </summary>
    [ObservableProperty]
    private TimeSpan storageRequestTimeout = TimeSpan.FromMinutes(2);

    /// <summary>
    /// Specify if token returned by factory will be used as "auth" url parameter or "access_token". 
    /// </summary>
    [ObservableProperty]
    private bool asAccessToken;

    /// <summary>
    /// Gets or sets the network state of the app.
    /// </summary>
    [ObservableProperty]
    private bool offlineMode;

    /// <summary>
    /// Gets or sets whether the <see cref="IEnumerable{T}"/> types will be serialized as blobs.
    /// </summary>
    [ObservableProperty]
    private bool databaseSerializeEnumerableAsBlobs;

    /// <summary>
    /// Gets or sets the firebase cloud firestore max concurrent writes.
    /// </summary>
    [ObservableProperty]
    private int cloudFirestoreMaxConcurrentSyncWrites = 100;

    /// <summary>
    /// Gets or sets the firebase realtime database max concurrent writes.
    /// </summary>
    [ObservableProperty]
    private int realtimeDatabaseMaxConcurrentSyncWrites = 100;

    /// <summary>
    /// Gets or sets the firebase realtime database max concurrent writes.
    /// </summary>
    [ObservableProperty]
    private JsonSerializerOptions databaseJsonSerializerOptions = new()
    {

    };
    
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
    }
}
