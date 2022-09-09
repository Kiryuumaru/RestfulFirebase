using RestfulFirebase.Auth;
using RestfulFirebase.RealtimeDatabase;
using RestfulFirebase.Local;
using RestfulFirebase.Storage;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using RestfulFirebase.CloudFirestore;
using DisposableHelpers;
using DisposableHelpers.Attributes;

namespace RestfulFirebase;

/// <summary>
/// App session for whole restful firebase operations.
/// </summary>
[Disposable]
public partial class RestfulFirebaseApp
{
    /// <summary>
    /// Gets <see cref="FirebaseConfig"/> of the app session 
    /// </summary>
    public FirebaseConfig Config { get; private set; }

    /// <summary>
    /// Gets the <see cref="LocalDatabaseApp"/> used for the app persistency.
    /// </summary>
    public LocalDatabaseApp LocalDatabase { get; private set; }

    /// <summary>
    /// Gets the <see cref="AuthApp"/> for firebase authentication app module.
    /// </summary>
    public AuthApp Auth { get; private set; }

    /// <summary>
    /// Gets the <see cref="CloudFirestoreApp"/> for firebase cloud firestore database app module.
    /// </summary>
    public CloudFirestoreApp CloudFirestore { get; private set; }

    /// <summary>
    /// Gets the <see cref="RealtimeDatabaseApp"/> for firebase realtime database app module.
    /// </summary>
    public RealtimeDatabaseApp RealtimeDatabase { get; private set; }

    /// <summary>
    /// Gets the <see cref="StorageApp"/> for firebase storage app module.
    /// </summary>
    public StorageApp Storage { get; private set; }

    internal static readonly JsonSerializerOptions DefaultJsonSerializerOption = new()
    {
        PropertyNameCaseInsensitive = true,
        IgnoreReadOnlyFields = true,
        NumberHandling = JsonNumberHandling.AllowReadingFromString
    };

    internal static readonly JsonDocumentOptions DefaultJsonDocumentOptions = new()
    {
        AllowTrailingCommas = true,
        CommentHandling = JsonCommentHandling.Skip
    };

    /// <summary>
    /// Creates new instance of <see cref="RestfulFirebaseApp"/> app.
    /// </summary>
    /// <param name="config">
    /// The <see cref="FirebaseConfig"/> configuration used by the app.
    /// </param>
    public RestfulFirebaseApp(FirebaseConfig config)
    {
        Config = config;
        LocalDatabase = new LocalDatabaseApp(this);
        CloudFirestore = new CloudFirestoreApp(this);
        RealtimeDatabase = new RealtimeDatabaseApp(this);
        Storage = new StorageApp(this);
        Auth = new AuthApp(this);
    }

    /// <summary>
    /// The dispose logic.
    /// </summary>
    /// <param name = "disposing">
    /// Whether the method is being called in response to disposal, or finalization.
    /// </param>
    protected void Dispose(bool disposing)
    {
        if (disposing)
        {
            Auth?.Dispose();
            CloudFirestore?.Dispose();
            RealtimeDatabase?.Dispose();
            Storage?.Dispose();
        }
    }
}
