namespace RestfulFirebase.FirestoreDatabase.References;

/// <summary>
/// The base reference of the cloud firestore.
/// </summary>
public abstract partial class Reference
{
    /// <summary>
    /// Gets the <see cref="FirebaseApp"/> used.
    /// </summary>
    internal FirebaseApp App { get; }

    internal Reference(FirebaseApp app)
    {
        App = app;
    }
}
