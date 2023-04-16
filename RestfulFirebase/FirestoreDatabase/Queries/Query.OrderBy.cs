using RestfulFirebase.FirestoreDatabase.Enums;
using RestfulFirebase.FirestoreDatabase.Utilities;

namespace RestfulFirebase.FirestoreDatabase.Queries;

public abstract partial class FluentQuery<TQuery>
{
    /// <summary>
    /// Adds new instance of <see cref="OrderByQuery"/> with <see cref="Direction.Ascending"/> order to the query.
    /// </summary>
    /// <param name="documentFieldPath">
    /// The order based on the document field path to order.
    /// </param>
    /// <returns>
    /// The query with new added "orderBy" query.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="documentFieldPath"/> is a null reference.
    /// </exception>
    /// <exception cref="System.ArgumentException">
    /// <paramref name="documentFieldPath"/> is empty.
    /// </exception>
    public TQuery Ascending(params string[] documentFieldPath)
    {
        ArgumentNullException.ThrowIfNull(documentFieldPath);
        ArgumentException.ThrowIfHasNullOrEmpty(documentFieldPath);

        TQuery query = (TQuery)Clone();

        query.WritableOrderByQuery.Add(new(documentFieldPath, false, Direction.Ascending));

        return query;
    }

    /// <summary>
    /// Adds new instance of <see cref="OrderByQuery"/> with <see cref="Direction.Descending"/> order to the query.
    /// </summary>
    /// <param name="documentFieldPath">
    /// The order based on the document field path.
    /// </param>
    /// <returns>
    /// The query with new added "orderBy" query.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="documentFieldPath"/> is a null reference.
    /// </exception>
    /// <exception cref="System.ArgumentException">
    /// <paramref name="documentFieldPath"/> is empty.
    /// </exception>
    public TQuery Descending(params string[] documentFieldPath)
    {
        ArgumentNullException.ThrowIfNull(documentFieldPath);
        ArgumentException.ThrowIfHasNullOrEmpty(documentFieldPath);

        TQuery query = (TQuery)Clone();

        query.WritableOrderByQuery.Add(new(documentFieldPath, false, Direction.Descending));

        return query;
    }

    /// <summary>
    /// Adds new instance of <see cref="OrderByQuery"/> with <see cref="Direction.Ascending"/> document name order to the query.
    /// </summary>
    /// <returns>
    /// The query with new added "orderBy" query.
    /// </returns>
    public TQuery AscendingDocumentName()
    {
        TQuery query = (TQuery)Clone();

        query.WritableOrderByQuery.Add(new(new string[] { DocumentFieldHelpers.DocumentName }, false, Direction.Ascending));

        return query;
    }

    /// <summary>
    /// Adds new instance of <see cref="OrderByQuery"/> with <see cref="Direction.Descending"/> document name order to the query.
    /// </summary>
    /// <returns>
    /// The query with new added "orderBy" query.
    /// </returns>
    public TQuery DescendingDocumentName()
    {
        TQuery query = (TQuery)Clone();

        query.WritableOrderByQuery.Add(new(new string[] { DocumentFieldHelpers.DocumentName }, false, Direction.Ascending));

        return query;
    }
}

public partial class FluentQuery<TQuery, TModel>
{
    /// <summary>
    /// Adds new instance of <see cref="OrderByQuery"/> with <see cref="Direction.Ascending"/> order to the query.
    /// </summary>
    /// <param name="propertyPath">
    /// The order based on the property path of the model to order.
    /// </param>
    /// <returns>
    /// The query with new added "orderBy" query.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="propertyPath"/> is a null reference.
    /// </exception>
    /// <exception cref="System.ArgumentException">
    /// <paramref name="propertyPath"/> is empty.
    /// </exception>
    public TQuery AscendingProperty(params string[] propertyPath)
    {
        ArgumentNullException.ThrowIfNull(propertyPath);
        ArgumentException.ThrowIfHasNullOrEmpty(propertyPath);

        TQuery query = (TQuery)Clone();

        query.WritableOrderByQuery.Add(new(propertyPath, true, Direction.Ascending));

        return query;
    }

    /// <summary>
    /// Adds new instance of <see cref="OrderByQuery"/> with <see cref="Direction.Descending"/> order to the query.
    /// </summary>
    /// <param name="propertyPath">
    /// The order based on the property path of the model.
    /// </param>
    /// <returns>
    /// The query with new added "orderBy" query.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="propertyPath"/> is a null reference.
    /// </exception>
    /// <exception cref="System.ArgumentException">
    /// <paramref name="propertyPath"/> is empty.
    /// </exception>
    public TQuery DescendingProperty(params string[] propertyPath)
    {
        ArgumentNullException.ThrowIfNull(propertyPath);
        ArgumentException.ThrowIfHasNullOrEmpty(propertyPath);

        TQuery query = (TQuery)Clone();

        query.WritableOrderByQuery.Add(new(propertyPath, true, Direction.Descending));

        return query;
    }
}

/// <summary>
/// The "orderBy" parameter for query.
/// </summary>
public class OrderByQuery
{
    /// <summary>
    /// Gets the path of the property or document field to order.
    /// </summary>
    public string[] NamePath { get; internal set; }

    /// <summary>
    /// Gets <c>true</c> if the <see cref="NamePath"/> is a property name; otherwise <c>false</c> if it is a document field name.
    /// </summary>
    public bool IsNamePathAPropertyPath { get; internal set; }

    /// <summary>
    /// Gets or sets the <see cref="Enums.Direction"/> of the order.
    /// </summary>
    public Direction Direction { get; internal set; }

    internal OrderByQuery(string[] namePath, bool isPathPropertyName, Direction direction)
    {
        NamePath = namePath;
        IsNamePathAPropertyPath = isPathPropertyName;
        Direction = direction;
    }
}

internal class StructuredOrderBy
{
    public OrderByQuery OrderByQuery { get; internal set; }

    public string DocumentFieldPath { get; internal set; }

    internal StructuredOrderBy(OrderByQuery orderByQuery, string documentFieldPath)
    {
        OrderByQuery = orderByQuery;
        DocumentFieldPath = documentFieldPath;
    }
}
