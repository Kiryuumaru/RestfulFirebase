using RestfulFirebase.FirestoreDatabase.Enums;
using RestfulFirebase.FirestoreDatabase.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RestfulFirebase.FirestoreDatabase.Queries;

public abstract partial class BaseQuery<TQuery>
{
    /// <summary>
    /// Adds new instance of <see cref="Queries.OrderByQuery"/> with <see cref="Direction.Ascending"/> order to the query.
    /// </summary>
    /// <param name="namePath">
    /// The order based on the property name of the model to order.
    /// </param>
    /// <returns>
    /// The query with new added "orderBy" query.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="namePath"/> is a null reference.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="namePath"/> is empty.
    /// </exception>
    public TQuery Ascending(params string[] namePath)
    {
        ArgumentNullException.ThrowIfNull(namePath);

        if (namePath.Length == 0)
        {
            throw new ArgumentException($"\"{nameof(namePath)}\" is empty.");
        }

        orderByQuery.Add(new(namePath, Direction.Ascending));

        return (TQuery)this;
    }

    /// <summary>
    /// Adds new instance of <see cref="Queries.OrderByQuery"/> with <see cref="Direction.Descending"/> order to the query.
    /// </summary>
    /// <param name="namePath">
    /// The order based on the property name of the model to order.
    /// </param>
    /// <returns>
    /// The query with new added "orderBy" query.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="namePath"/> is a null reference.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="namePath"/> is empty.
    /// </exception>
    public TQuery Descending(params string[] namePath)
    {
        ArgumentNullException.ThrowIfNull(namePath);

        if (namePath.Length == 0)
        {
            throw new ArgumentException($"\"{nameof(namePath)}\" is empty.");
        }

        orderByQuery.Add(new(namePath, Direction.Descending));

        return (TQuery)this;
    }

    /// <summary>
    /// Adds new instance of <see cref="Queries.OrderByQuery"/> with <see cref="Direction.Ascending"/> document name order to the query.
    /// </summary>
    /// <returns>
    /// The query with new added "orderBy" query.
    /// </returns>
    public TQuery AscendingDocumentName()
    {
        orderByQuery.Add(new(new string[] { DocumentFieldHelpers.DocumentName }, Direction.Ascending));

        return (TQuery)this;
    }

    /// <summary>
    /// Adds new instance of <see cref="Queries.OrderByQuery"/> with <see cref="Direction.Descending"/> document name order to the query.
    /// </summary>
    /// <returns>
    /// The query with new added "orderBy" query.
    /// </returns>
    public TQuery DescendingDocumentName()
    {
        orderByQuery.Add(new(new string[] { DocumentFieldHelpers.DocumentName }, Direction.Ascending));

        return (TQuery)this;
    }
}

/// <summary>
/// The "orderBy" parameter for query.
/// </summary>
public class OrderByQuery
{
    /// <summary>
    /// Gets or sets the order based on the document field path to order.
    /// </summary>
    public string[] NamePath { get; internal set; }

    /// <summary>
    /// Gets or sets the <see cref="Enums.Direction"/> of the order.
    /// </summary>
    public Direction Direction { get; internal set; }

    internal OrderByQuery(string[] namePath, Direction direction)
    {
        NamePath = namePath;
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
