namespace RestfulFirebase.FirestoreDatabase.References;

/// <summary>
/// The reference for collections.
/// </summary>
public partial class CollectionReference : Reference
{
    /// <summary>
    /// Gets the ID of the reference.
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// Gets the parent of the collection reference. returns a <c>null</c> reference if the collection reference is a root collection reference.
    /// </summary>
    public DocumentReference? Parent { get; }

    internal CollectionReference(FirebaseApp app, string id, DocumentReference? parent)
        : base(app)
    {
        Id = id;
        Parent = parent;
    }
}
