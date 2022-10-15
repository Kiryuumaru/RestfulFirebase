using RestfulFirebase.FirestoreDatabase.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics.CodeAnalysis;

namespace RestfulFirebase.FirestoreDatabase.Queries;

public abstract partial class BaseQuery<TQuery>
{
    /// <summary>
    /// Adds the <see cref="Queries.SelectQuery"/> to the query.
    /// </summary>
    /// <param name="documentFieldPath">
    /// The document field path to add.
    /// </param>
    /// <returns>
    /// The query with new added "select" query.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="documentFieldPath"/> is a null reference.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="documentFieldPath"/> is empty.
    /// </exception>
    public virtual TQuery Select(params string[] documentFieldPath)
    {
        ArgumentNullException.ThrowIfNull(documentFieldPath);

        if (documentFieldPath.Length == 0)
        {
            throw new ArgumentException($"\"{nameof(documentFieldPath)}\" is empty.");
        }

        selectQuery.Add(new(documentFieldPath, false));

        return (TQuery)this;
    }

    /// <summary>
    /// Adds new instance of <see cref="Queries.SelectQuery"/> with '__name__' document name to only return the name of the document.
    /// </summary>
    /// <returns>
    /// The query with new added "select" query.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Select query already contains field projections.
    /// </exception>
    public TQuery SelectDocumentNameOnly()
    {
        if (SelectQuery.Count != 0)
        {
            throw new ArgumentException("Select query already contains field projections.");
        }

        selectQuery.Add(new(new string[] { DocumentFieldHelpers.DocumentName }, false));

        return (TQuery)this;
    }
}

public partial class Query<TModel> : BaseQuery<Query<TModel>>
{
    /// <summary>
    /// Adds the <see cref="SelectQuery"/> to the query.
    /// </summary>
    /// <param name="propertyPath">
    /// The property path to add.
    /// </param>
    /// <returns>
    /// The query with new added "select" query.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="propertyPath"/> is a null reference.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="propertyPath"/> is empty.
    /// </exception>
    public override Query<TModel> Select(params string[] propertyPath)
        => SelectProperty(propertyPath);

    /// <summary>
    /// Adds the <see cref="SelectQuery"/> to the query.
    /// </summary>
    /// <param name="documentFieldPath">
    /// The document field path to add.
    /// </param>
    /// <returns>
    /// The query with new added "select" query.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="documentFieldPath"/> is a null reference.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="documentFieldPath"/> is empty.
    /// </exception>
    public Query<TModel> SelectDocumentField(params string[] documentFieldPath)
        => base.Select(documentFieldPath);

    /// <summary>
    /// Adds the <see cref="SelectQuery"/> to the query.
    /// </summary>
    /// <param name="propertyPath">
    /// The property path to add.
    /// </param>
    /// <returns>
    /// The query with new added "select" query.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="propertyPath"/> is a null reference.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="propertyPath"/> is empty.
    /// </exception>
    public Query<TModel> SelectProperty(params string[] propertyPath)
    {
        ArgumentNullException.ThrowIfNull(propertyPath);

        if (propertyPath.Length == 0)
        {
            throw new ArgumentException($"\"{nameof(propertyPath)}\" is empty.");
        }

        selectQuery.Add(new(propertyPath, true));

        return this;
    }
}

/// <summary>
/// The "select" parameter for query.
/// </summary>
public class SelectQuery
{
    /// <summary>
    /// Gets the path of the document field the projection will return.
    /// </summary>
    public string[] NamePath { get; internal set; }

    /// <summary>
    /// Gets <c>true</c> if the <see cref="NamePath"/> is a property name; otherwise <c>false</c> if it is a document field name.
    /// </summary>
    public bool IsNamePathAPropertyPath { get; internal set; }

    internal SelectQuery(string[] namePath, bool isPathPropertyName)
    {
        NamePath = namePath;
        IsNamePathAPropertyPath = isPathPropertyName;
    }
}

internal class StructuredSelect
{
    public SelectQuery? SelectQuery { get; internal set; }

    public string DocumentFieldPath { get; internal set; }

    internal StructuredSelect(SelectQuery selectQuery, string documentFieldPath)
    {
        SelectQuery = selectQuery;
        DocumentFieldPath = documentFieldPath;
    }
}
