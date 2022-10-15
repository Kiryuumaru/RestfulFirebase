using RestfulFirebase.FirestoreDatabase.Enums;
using RestfulFirebase.FirestoreDatabase.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace RestfulFirebase.FirestoreDatabase.Queries;

public abstract partial class BaseQuery<TQuery>
{
    /// <summary>
    /// Adds new instance of <see cref="Queries.OrderByQuery"/> with <see cref="Direction.Ascending"/> order to the query.
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
    /// <exception cref="ArgumentException">
    /// <paramref name="documentFieldPath"/> is empty.
    /// </exception>
    public virtual TQuery Ascending(params string[] documentFieldPath)
    {
        ArgumentNullException.ThrowIfNull(documentFieldPath);

        if (documentFieldPath.Length == 0)
        {
            throw new ArgumentException($"\"{nameof(documentFieldPath)}\" is empty.");
        }

        orderByQuery.Add(new(documentFieldPath, false, Direction.Ascending));

        return (TQuery)this;
    }

    /// <summary>
    /// Adds new instance of <see cref="Queries.OrderByQuery"/> with <see cref="Direction.Descending"/> order to the query.
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
    /// <exception cref="ArgumentException">
    /// <paramref name="documentFieldPath"/> is empty.
    /// </exception>
    public virtual TQuery Descending(params string[] documentFieldPath)
    {
        ArgumentNullException.ThrowIfNull(documentFieldPath);

        if (documentFieldPath.Length == 0)
        {
            throw new ArgumentException($"\"{nameof(documentFieldPath)}\" is empty.");
        }

        orderByQuery.Add(new(documentFieldPath, false, Direction.Descending));

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
        orderByQuery.Add(new(new string[] { DocumentFieldHelpers.DocumentName }, false, Direction.Ascending));

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
        orderByQuery.Add(new(new string[] { DocumentFieldHelpers.DocumentName }, false, Direction.Ascending));

        return (TQuery)this;
    }
}

public partial class Query<TModel> : BaseQuery<Query<TModel>>
{
    /// <summary>
    /// Adds new instance of <see cref="OrderByQuery"/> with <see cref="Direction.Ascending"/> order to the query.
    /// </summary>
    /// <param name="propertyPath">
    /// The order based on the property name of the model to order.
    /// </param>
    /// <returns>
    /// The query with new added "orderBy" query.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="propertyPath"/> is a null reference.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="propertyPath"/> is empty.
    /// </exception>
    public override Query<TModel> Ascending(params string[] propertyPath)
        => AscendingProperty(propertyPath);

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
    /// <exception cref="ArgumentException">
    /// <paramref name="documentFieldPath"/> is empty.
    /// </exception>
    public Query<TModel> AscendingDocumentField(params string[] documentFieldPath)
        => base.Ascending(documentFieldPath);

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
    /// <exception cref="ArgumentException">
    /// <paramref name="propertyPath"/> is empty.
    /// </exception>
    public Query<TModel> AscendingProperty(params string[] propertyPath)
    {
        ArgumentNullException.ThrowIfNull(propertyPath);

        if (propertyPath.Length == 0)
        {
            throw new ArgumentException($"\"{nameof(propertyPath)}\" is empty.");
        }

        orderByQuery.Add(new(propertyPath, true, Direction.Ascending));

        return this;
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
    /// <exception cref="ArgumentException">
    /// <paramref name="propertyPath"/> is empty.
    /// </exception>
    public override Query<TModel> Descending(params string[] propertyPath)
        => DescendingProperty(propertyPath);

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
    /// <exception cref="ArgumentException">
    /// <paramref name="documentFieldPath"/> is empty.
    /// </exception>
    public Query<TModel> DescendingDocumentField(params string[] documentFieldPath)
        => base.Descending(documentFieldPath);

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
    /// <exception cref="ArgumentException">
    /// <paramref name="propertyPath"/> is empty.
    /// </exception>
    public Query<TModel> DescendingProperty(params string[] propertyPath)
    {
        ArgumentNullException.ThrowIfNull(propertyPath);

        if (propertyPath.Length == 0)
        {
            throw new ArgumentException($"\"{nameof(propertyPath)}\" is empty.");
        }

        orderByQuery.Add(new(propertyPath, true, Direction.Descending));

        return this;
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
