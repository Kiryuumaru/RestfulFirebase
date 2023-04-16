using System.Linq;

namespace RestfulFirebase.FirestoreDatabase.Queries;

public partial class FluentQuery<TQuery>
{
    /// <summary>
    /// Adds an instance of <see cref="FromQuery"/> to the query.
    /// </summary>
    /// <param name="collectionIds">
    /// The ID of the collection references.
    /// </param>
    /// <returns>
    /// The query with new added "from" query.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="collectionIds"/> is a <c>null</c> reference.
    /// </exception>
    /// <exception cref="System.ArgumentException">
    /// <paramref name="collectionIds"/> is empty.
    /// </exception>
    public TQuery From(params string[] collectionIds)
    {
        ArgumentNullException.ThrowIfNull(collectionIds);
        ArgumentException.ThrowIfHasNullOrEmpty(collectionIds);

        TQuery query = (TQuery)Clone();

        query.WritableFromQuery.AddRange(collectionIds.Select(id => new FromQuery(id, DocumentReference == null)));

        return query;
    }

    /// <summary>
    /// Adds an instance of <see cref="FromQuery"/> to the query.
    /// </summary>
    /// <param name="allDescendants">
    /// When <c>false</c>, selects only collections that are immediate children of the parent specified in the containing RunQueryRequest. When <c>true</c>, selects all descendant collections.
    /// </param>
    /// <param name="collectionIds">
    /// The ID of the collection references.
    /// </param>
    /// <returns>
    /// The query with new added "from" query.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="collectionIds"/> is a <c>null</c> reference.
    /// </exception>
    /// <exception cref="System.ArgumentException">
    /// <paramref name="collectionIds"/> is empty or
    /// <paramref name="allDescendants"/> is <c>true</c> and query is not in the root query.
    /// </exception>
    public TQuery From(bool allDescendants, params string[] collectionIds)
    {
        ArgumentNullException.ThrowIfNull(collectionIds);
        ArgumentException.ThrowIfHasNullOrEmpty(collectionIds);
        if (allDescendants && DocumentReference != null)
        {
            ArgumentException.Throw($"\"{nameof(allDescendants)}\" is only applicable from root query.");
        }

        TQuery query = (TQuery)Clone();

        query.WritableFromQuery.AddRange(collectionIds.Select(id => new FromQuery(id, allDescendants)));

        return query;
    }
}

/// <summary>
/// The "from" parameter for query.
/// </summary>
public class FromQuery
{
    /// <summary>
    /// Gets or sets the ID of the collection. When set, selects only collections with this ID.
    /// </summary>
    public string CollectionId { get; internal set; }

    /// <summary>
    /// Gets or sets <c>true</c> whether to select all descendant collections; otherwise, <c>false</c> to select only collections that are immediate children of the parent specified in the containing request. 
    /// </summary>
    public bool AllDescendants { get; internal set; }

    internal FromQuery(string collectionId, bool allDescendants)
    {
        ArgumentNullException.ThrowIfNull(collectionId);

        CollectionId = collectionId;
        AllDescendants = allDescendants;
    }
}

internal class StructuredFrom
{
    public FromQuery FromQuery { get; internal set; }

    internal StructuredFrom(FromQuery fromQuery)
    {
        FromQuery = fromQuery;
    }
}
