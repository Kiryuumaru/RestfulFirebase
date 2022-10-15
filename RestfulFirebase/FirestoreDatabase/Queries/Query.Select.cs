using RestfulFirebase.FirestoreDatabase.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RestfulFirebase.FirestoreDatabase.Queries;

public abstract partial class BaseQuery<TQuery>
{
    /// <summary>
    /// Adds the <see cref="Queries.SelectQuery"/> to the query.
    /// </summary>
    /// <param name="namePath">
    /// The property name path to add.
    /// </param>
    /// <returns>
    /// The query with new added "select" query.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="namePath"/> is a null reference.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="namePath"/> is empty.
    /// </exception>
    public TQuery Select(params string[] namePath)
    {
        ArgumentNullException.ThrowIfNull(namePath);

        if (namePath.Length == 0)
        {
            throw new ArgumentException($"\"{nameof(namePath)}\" is empty.");
        }

        selectQuery.Add(new(new string[] { DocumentFieldHelpers.DocumentName }));

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
    public TQuery SelectNameOnly()
    {
        if (SelectQuery.Count != 0)
        {
            throw new ArgumentException("Select query already contains field projections.");
        }

        selectQuery.Add(new(new string[] { DocumentFieldHelpers.DocumentName }));

        return (TQuery)this;
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

    internal SelectQuery(string[] namePath)
    {
        NamePath = namePath;
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
