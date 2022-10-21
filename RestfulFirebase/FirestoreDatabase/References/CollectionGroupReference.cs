using System.Collections.Generic;

namespace RestfulFirebase.FirestoreDatabase.References;

/// <summary>
/// The reference for collections.
/// </summary>
public partial class CollectionGroupReference : Reference
{
    /// <summary>
    /// Gets the ID of the collection references.
    /// </summary>
    public IReadOnlyList<string> AllDescendants { get; }

    /// <summary>
    /// Gets the ID of the collection references.
    /// </summary>
    public IReadOnlyList<string> DirectDescendants { get; }

    /// <summary>
    /// Gets the parent of the collection reference. returns a <c>null</c> reference if the collection reference is a root collection reference.
    /// </summary>
    public DocumentReference? Parent { get; }

    internal readonly List<string> WritableAllDescendants;
    internal readonly List<string> WritableDirectDescendants;

    internal CollectionGroupReference(FirebaseApp app, DocumentReference? parent)
        : base(app)
    {
        WritableAllDescendants = new();
        WritableDirectDescendants = new();
        AllDescendants = WritableAllDescendants.AsReadOnly();
        DirectDescendants = WritableDirectDescendants.AsReadOnly();

        Parent = parent;
    }
}