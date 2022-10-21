using RestfulFirebase.FirestoreDatabase;
using RestfulFirebase.Authentication;

namespace RestfulFirebase;

/// <summary>
/// App session for whole firebase operations.
/// </summary>
public partial class FirebaseApp
{
    /// <summary>
    /// Gets <see cref="FirebaseConfig"/> configuration used by the app.
    /// </summary>
    public FirebaseConfig Config { get; }

    /// <summary>
    /// Gets the <see cref="AuthenticationApi"/> for firebase authentication operations.
    /// </summary>
    public AuthenticationApi Authentication { get; }

    /// <summary>
    /// Gets the <see cref="AuthenticationApi"/> for firebase firestore database operations.
    /// </summary>
    public FirestoreDatabaseApi FirestoreDatabase { get; }

    /// <summary>
    /// Creates new instance of <see cref="FirebaseApp"/> app.
    /// </summary>
    /// <param name="config">
    /// The <see cref="FirebaseConfig"/> configuration used by the app.
    /// </param>
    public FirebaseApp(FirebaseConfig config)
    {
        Config = config;
        Authentication = new(this);
        FirestoreDatabase = new(this);
    }
}
