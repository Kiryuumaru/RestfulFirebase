using RestfulFirebase.Common.Internals;
using RestfulFirebase.FirestoreDatabase.Models;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace RestfulFirebase.FirestoreDatabase.References;

/// <summary>
/// The reference for collections.
/// </summary>
public partial class CollectionGroupReference : Reference
{
    /// <summary>
    /// Gets the ID of the reference.
    /// </summary>
    public CollectionReference[] CollectionReferences { get; }

    /// <summary>
    /// Gets the parent of the collection reference. returns a <c>null</c> reference if the collection reference is a root collection reference.
    /// </summary>
    public DocumentReference? Parent { get; }

    internal CollectionGroupReference(FirebaseApp app, string[] ids, DocumentReference? parent)
        : base(app)
    {
        CollectionReferences = new CollectionReference[ids.Length];
        for (int i = 0; i < ids.Length; i++)
        {
            CollectionReferences[i] = new(app, ids[i], Parent);
        }
        Parent = parent;
    }
}