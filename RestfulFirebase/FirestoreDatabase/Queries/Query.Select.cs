using RestfulFirebase.FirestoreDatabase.Utilities;

namespace RestfulFirebase.FirestoreDatabase.Queries;

public abstract partial class FluentQuery<TQuery>
{
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
    /// <exception cref="System.ArgumentException">
    /// <paramref name="documentFieldPath"/> is empty.
    /// </exception>
    public TQuery Select(params string[] documentFieldPath)
    {
        ArgumentNullException.ThrowIfNull(documentFieldPath);
        ArgumentException.ThrowIfHasNullOrEmpty(documentFieldPath);

        TQuery query = (TQuery)Clone();

        query.WritableSelectQuery.Add(new(documentFieldPath, false));

        return query;
    }

    /// <summary>
    /// Adds new instance of <see cref="SelectQuery"/> with '__name__' document name to only return the name of the document.
    /// </summary>
    /// <returns>
    /// The query with new added "select" query.
    /// </returns>
    /// <exception cref="System.ArgumentException">
    /// Select query already contains field projections.
    /// </exception>
    public TQuery SelectDocumentNameOnly()
    {
        if (SelectQuery.Count != 0)
        {
            ArgumentException.Throw("Select query already contains field projections.");
        }

        TQuery query = (TQuery)Clone();

        query.WritableSelectQuery.Add(new(new string[] { DocumentFieldHelpers.DocumentName }, false));

        return query;
    }
}

public partial class FluentQuery<TQuery, TModel>
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
    /// <exception cref="System.ArgumentException">
    /// <paramref name="propertyPath"/> is empty.
    /// </exception>
    public TQuery SelectProperty(params string[] propertyPath)
    {
        ArgumentNullException.ThrowIfNull(propertyPath);
        ArgumentException.ThrowIfHasNullOrEmpty(propertyPath);

        TQuery query = (TQuery)Clone();

        query.WritableSelectQuery.Add(new(propertyPath, true));

        return query;
    }
}

/// <summary>
/// The "select" parameter for query.
/// </summary>
public class SelectQuery
{
    /// <summary>
    /// Gets the path of the property or document field the projection will return.
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
