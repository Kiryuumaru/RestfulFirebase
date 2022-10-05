using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using RestfulFirebase.Common.Internals;
using RestfulFirebase.FirestoreDatabase.Models;

namespace RestfulFirebase.FirestoreDatabase.References;

/// <summary>
/// The reference for documents.
/// </summary>
public partial class DocumentReference : Reference
{
    /// <summary>
    /// Gets the ID of the reference.
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// Gets the parent of the document reference.
    /// </summary>
    public CollectionReference Parent { get; }

    internal DocumentReference(FirebaseApp app, string id, CollectionReference parent)
        : base(app)
    {
        Id = id;
        Parent = parent;
    }
}
