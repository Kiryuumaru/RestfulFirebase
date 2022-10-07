using RestfulFirebase.Common.Internals;
using RestfulFirebase.FirestoreDatabase.Models;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.Json;

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

    private readonly List<string> allDescendants;
    private readonly List<string> directDescendants;

    internal CollectionGroupReference(FirebaseApp app, DocumentReference? parent)
        : base(app)
    {
        allDescendants = new();
        directDescendants = new();
        AllDescendants = allDescendants.AsReadOnly();
        DirectDescendants = directDescendants.AsReadOnly();

        Parent = parent;
    }
}