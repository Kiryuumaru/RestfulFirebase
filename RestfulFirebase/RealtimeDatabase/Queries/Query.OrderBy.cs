using RestfulFirebase.RealtimeDatabase.Enums;
using System;

namespace RestfulFirebase.RealtimeDatabase.Queries;

public partial class FluentQuery<TQuery>
{
    /// <summary>
    /// Order data by given <paramref name="propertyName"/>. Note that this is used mainly for following filtering queries and due to firebase implementation
    /// the data may actually not be ordered.
    /// </summary>
    /// <param name="propertyName">
    /// The property name.
    /// </param>
    /// <returns>
    /// The query with new added order.
    /// </returns>
    public TQuery OrderBy(string propertyName)
    {
        ArgumentNullException.ThrowIfNull(propertyName);

        TQuery query = (TQuery)Clone();

        query.WritableOrderByQuery.Add(new OrderByQuery(propertyName));

        return query;
    }

    /// <summary>
    /// Order data by $key. Note that this is used mainly for following filtering queries and due to firebase implementation
    /// the data may actually not be ordered.
    /// </summary>
    /// <returns>
    /// The query with new added order.
    /// </returns>
    public TQuery OrderByKey()
    {
        return OrderBy("$key");
    }

    /// <summary>
    /// Order data by $value. Note that this is used mainly for following filtering queries and due to firebase implementation
    /// the data may actually not be ordered.
    /// </summary>
    /// <returns>
    /// The query with new added order.
    /// </returns>
    public TQuery OrderByValue()
    {
        return OrderBy("$value");
    }

    /// <summary>
    /// Order data by $priority. Note that this is used mainly for following filtering queries and due to firebase implementation
    /// the data may actually not be ordered.
    /// </summary>
    /// <returns>
    /// The query with new added order.
    /// </returns>
    public TQuery OrderByPriority()
    {
        return OrderBy("$priority");
    }
}

public partial class FluentQuery<TQuery, TModel>
{

}

/// <summary>
/// The "where" parameter for query.
/// </summary>
public class OrderByQuery
{
    /// <summary>
    /// Gets the path of the property name to order.
    /// </summary>
    public string PropertyName { get; internal set; }

    internal OrderByQuery(string propertyName)
    {
        PropertyName = propertyName;
    }
}
