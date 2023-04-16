using RestfulFirebase.RealtimeDatabase.Enums;

namespace RestfulFirebase.RealtimeDatabase.Queries;

public partial class FluentQuery<TQuery>
{

}

public partial class FluentQuery<TQuery, TModel>
{

}

/// <summary>
/// The "where" parameter for query.
/// </summary>
public abstract class FilterQuery
{
    /// <summary>
    /// Gets the path of the property or document field to filter.
    /// </summary>
    public string[] NamePath { get; internal set; }

    /// <summary>
    /// Gets <c>true</c> if the <see cref="NamePath"/> is a property name; otherwise <c>false</c> if it is a document field name.
    /// </summary>
    public bool IsNamePathAPropertyPath { get; internal set; }

    internal FilterQuery(string[] namePath, bool isPathPropertyName)
    {
        NamePath = namePath;
        IsNamePathAPropertyPath = isPathPropertyName;
    }
}

internal class StructuredFilter
{
    public FilterQuery FilterQuery { get; internal set; }

    public string DocumentFieldPath { get; internal set; }

    internal StructuredFilter(FilterQuery filterQuery, string documentFieldPath)
    {
        FilterQuery = filterQuery;
        DocumentFieldPath = documentFieldPath;
    }
}
