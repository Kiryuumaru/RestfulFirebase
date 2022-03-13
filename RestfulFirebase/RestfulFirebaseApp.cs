using ObservableHelpers;
using ObservableHelpers.Utilities;
using RestfulFirebase.Auth;
using RestfulFirebase.Database;
using RestfulFirebase.Local;
using RestfulFirebase.Storage;
using SynchronizationContextHelpers;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RestfulFirebase;

/// <summary>
/// App session for whole restful firebase operations.
/// </summary>
public class RestfulFirebaseApp : SyncContext
{
    #region Properties

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
    /// Gets the <see cref="DatabaseApp"/> for firebase database app module.
    /// </summary>
    public DatabaseApp Database { get; private set; }

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

    #endregion

    #region Initializers

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
        Database = new DatabaseApp(this);
        Storage = new StorageApp(this);
        Auth = new AuthApp(this);
    }

    #endregion

    #region Methods



    #endregion

    #region Disposable Members

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            Auth?.Dispose();
            Database?.Dispose();
            Storage?.Dispose();
        }
        base.Dispose(disposing);
    }

    #endregion
}
