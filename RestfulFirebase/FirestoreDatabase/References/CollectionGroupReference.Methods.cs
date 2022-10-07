using RestfulFirebase.Common.Internals;
using RestfulFirebase.FirestoreDatabase.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Threading;
using RestfulFirebase.Common.Http;
using RestfulFirebase.FirestoreDatabase.Transactions;
using RestfulFirebase.Common.Abstractions;

namespace RestfulFirebase.FirestoreDatabase.References;

public partial class CollectionGroupReference : Reference
{
    /// <inheritdoc/>
    public override bool Equals(object? obj)
    {
        return obj is CollectionGroupReference reference &&
               EqualityComparer<IReadOnlyList<(bool, string)>>.Default.Equals(Ids, reference.Ids) &&
               EqualityComparer<DocumentReference?>.Default.Equals(Parent, reference.Parent);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        int hashCode = 1488852771;
        hashCode = hashCode * -1521134295 + EqualityComparer<IReadOnlyList<(bool, string)>>.Default.GetHashCode(Ids);
        hashCode = hashCode * -1521134295 + (Parent == null ? 0 : EqualityComparer<DocumentReference?>.Default.GetHashCode(Parent));
        return hashCode;
    }

    /// <summary>
    /// Adds a collection reference <see cref="CollectionReference"/>.
    /// </summary>
    /// <param name="collectionIds">
    /// The ID of the collection references.
    /// </param>
    /// <returns>
    /// The <see cref="CollectionReference"/> of the specified <paramref name="collectionIds"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="collectionIds"/> is a <c>null</c> reference.
    /// </exception>
    public CollectionGroupReference AddCollection(string[] collectionIds)
    {
        ArgumentNullException.ThrowIfNull(collectionIds);

        ids.AddRange(collectionIds.Select(id => (true, id)));

        return this;
    }

    /// <summary>
    /// Adds a collection reference <see cref="CollectionReference"/>.
    /// </summary>
    /// <param name="allDescendants">
    /// When <c>false</c>, selects only collections that are immediate children of the parent specified in the containing RunQueryRequest. When <c>true</c>, selects all descendant collections.
    /// </param>
    /// <param name="collectionIds">
    /// The ID of the collection references.
    /// </param>
    /// <returns>
    /// The <see cref="CollectionReference"/> of the specified <paramref name="collectionIds"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="collectionIds"/> is a <c>null</c> reference.
    /// </exception>
    public CollectionGroupReference AddCollection(bool allDescendants, string[] collectionIds)
    {
        ArgumentNullException.ThrowIfNull(collectionIds);

        ids.AddRange(collectionIds.Select(id => (allDescendants, id)));

        return this;
    }
}
