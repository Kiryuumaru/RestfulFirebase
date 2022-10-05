namespace RestfulFirebase.FirestoreDatabase.References;

/// <summary>
/// The base reference of the cloud firestore.
/// </summary>
public abstract class Reference
{
    /// <summary>
    /// Gets the <see cref="FirebaseApp"/> used.
    /// </summary>
    public FirebaseApp App { get; }

    #region Properties

    internal Reference(FirebaseApp app)
    {
        App = app;
    }

    #endregion
}
