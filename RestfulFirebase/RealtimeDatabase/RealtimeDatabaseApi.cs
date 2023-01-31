namespace RestfulFirebase.RealtimeDatabase;

/// <summary>
/// Provides firebase realtime database implementations.
/// </summary>
public partial class RealtimeDatabaseApi
{
    /// <summary>
    /// Gets the <see cref="FirebaseApp"/> used.
    /// </summary>
    internal FirebaseApp App { get; }

    internal RealtimeDatabaseApi(FirebaseApp app)
    {
        App = app;
    }
}
