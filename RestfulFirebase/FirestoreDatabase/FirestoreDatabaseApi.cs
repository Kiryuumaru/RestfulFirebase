

namespace RestfulFirebase.FirestoreDatabase;

/// <summary>
/// Provides firebase cloud firestore database implementations.
/// </summary>
public partial class FirestoreDatabaseApi
{
    /// <summary>
    /// Gets the <see cref="FirebaseApp"/> used.
    /// </summary>
    public FirebaseApp App { get; }

    internal FirestoreDatabaseApi(FirebaseApp app)
    {
        App = app;
    }
}
