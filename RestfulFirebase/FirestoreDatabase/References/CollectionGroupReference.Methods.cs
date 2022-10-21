using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using RestfulFirebase.FirestoreDatabase.Queries;

namespace RestfulFirebase.FirestoreDatabase.References;

public partial class CollectionGroupReference : Reference
{
    /// <inheritdoc/>
    public override bool Equals(object? obj)
    {
        return obj is CollectionGroupReference reference &&
               EqualityComparer<IReadOnlyList<string>>.Default.Equals(AllDescendants, reference.AllDescendants) &&
               EqualityComparer<IReadOnlyList<string>>.Default.Equals(DirectDescendants, reference.DirectDescendants) &&
               EqualityComparer<DocumentReference?>.Default.Equals(Parent, reference.Parent);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        int hashCode = 1488852771;
        hashCode = hashCode * -1521134295 + EqualityComparer<IReadOnlyList<string>>.Default.GetHashCode(AllDescendants);
        hashCode = hashCode * -1521134295 + EqualityComparer<IReadOnlyList<string>>.Default.GetHashCode(DirectDescendants);
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

        this.WritableAllDescendants.AddRange(collectionIds);

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

        if (allDescendants)
        {
            this.WritableAllDescendants.AddRange(collectionIds);
        }
        else
        {
            WritableDirectDescendants.AddRange(collectionIds);
        }

        return this;
    }

    /// <summary>
    /// Creates a structured <see cref="QueryRoot"/>.
    /// </summary>
    /// <returns>
    /// The created structured <see cref="QueryRoot"/>
    /// </returns>
    public QueryRoot Query()
    {
        QueryRoot query = new(App, null, Parent);

        if (AllDescendants.Count != 0)
        {
            query.From(true, AllDescendants.ToArray());
        }
        if (DirectDescendants.Count != 0)
        {
            query.From(false, DirectDescendants.ToArray());
        }

        return query;
    }

    /// <summary>
    /// Creates a structured <see cref="QueryRoot{TModel}"/>.
    /// </summary>
    /// <typeparam name="TModel">
    /// The type of the document model.
    /// </typeparam>
    /// <returns>
    /// The created structured <see cref="QueryRoot{TModel}"/>
    /// </returns>
    public QueryRoot<TModel> Query<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TModel>()
        where TModel : class
    {
        QueryRoot<TModel> query = new(App, Parent);

        if (AllDescendants.Count != 0)
        {
            query.From(true, AllDescendants.ToArray());
        }
        if (DirectDescendants.Count != 0)
        {
            query.From(false, DirectDescendants.ToArray());
        }

        return query;
    }
}
