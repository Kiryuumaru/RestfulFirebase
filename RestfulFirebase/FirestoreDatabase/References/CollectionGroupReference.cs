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
    public string[] Ids { get; }

    /// <summary>
    /// Gets the parent of the collection reference. returns a <c>null</c> reference if the collection reference is a root collection reference.
    /// </summary>
    public DocumentReference? Parent { get; }

    internal CollectionGroupReference(FirebaseApp app, string[] ids, DocumentReference? parent)
        : base(app)
    {
        Ids = ids;
        Parent = parent;
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj)
    {
        return obj is CollectionGroupReference reference &&
               EqualityComparer<string[]>.Default.Equals(Ids, reference.Ids) &&
               EqualityComparer<DocumentReference?>.Default.Equals(Parent, reference.Parent);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        int hashCode = 1488852771;
        hashCode = hashCode * -1521134295 + EqualityComparer<string[]>.Default.GetHashCode(Ids);
        hashCode = hashCode * -1521134295 + (Parent == null ? 0 : EqualityComparer<DocumentReference?>.Default.GetHashCode(Parent));
        return hashCode;
    }
}

/// <inheritdoc/>
/// <typeparam name="TModel">
/// The type of the document model of the collection.
/// </typeparam>
public partial class CollectionGroupReference<TModel> : CollectionGroupReference
    where TModel : class
{
    internal CollectionGroupReference(FirebaseApp app, string[] ids, DocumentReference? parent)
        : base(app, ids, parent)
    {

    }
}
