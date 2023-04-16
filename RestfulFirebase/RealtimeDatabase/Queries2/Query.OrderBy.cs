using RestfulFirebase.Common.Abstractions;
using RestfulHelpers.Common;
using RestfulFirebase.RealtimeDatabase.References;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace RestfulFirebase.RealtimeDatabase.Queries2;

public partial class FluentOrderedQuery<TQuery>
{
    /// <summary>
    /// Order data by given <paramref name="propertyNameFactory"/>. Note that this is used mainly for following filtering queries and due to firebase implementation
    /// the data may actually not be ordered.
    /// </summary>
    /// <param name="propertyNameFactory">
    /// The property name factory.
    /// </param>
    /// <returns>
    /// The query with new added order.
    /// </returns>
    public FilteredQuery OrderBy(Func<string> propertyNameFactory)
    {
        ArgumentNullException.ThrowIfNull(propertyNameFactory);

        FilteredQuery query = new(Reference, this, ct =>
        {
            HttpResponse<string> response = new();

            string propertyName = propertyNameFactory();

            if (string.IsNullOrEmpty(propertyName))
            {
                response.Append($"");
            }
            else
            {
                response.Append($"orderBy={propertyName}");
            }

            return new(response);
        });

        return query;
    }

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
    public FilteredQuery OrderBy(string propertyName)
    {
        return OrderBy(() => propertyName);
    }

    /// <summary>
    /// Order data by $key. Note that this is used mainly for following filtering queries and due to firebase implementation
    /// the data may actually not be ordered.
    /// </summary>
    /// <returns>
    /// The query with new added order.
    /// </returns>
    public FilteredQuery OrderByKey()
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
    public FilteredQuery OrderByValue()
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
    public FilteredQuery OrderByPriority()
    {
        return OrderBy("$priority");
    }
}
