using RestfulFirebase.Common.Abstractions;
using RestfulFirebase.RealtimeDatabase.References;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace RestfulFirebase.RealtimeDatabase.Queries;

public partial class FluentFilteredQuery<TQuery>
{
    /// <summary>
    /// Limits the result to first <paramref name="countFactory"/> items.
    /// </summary>
    /// <param name="countFactory">
    /// Number of elements.
    /// </param>
    /// <returns>
    /// The query with new added filter.
    /// </returns>
    public TQuery LimitToFirst(Func<int> countFactory)
    {
        return FilterCore("limitToFirst", () => countFactory());
    }

    /// <summary>
    /// Limits the result to first <paramref name="count"/> items.
    /// </summary>
    /// <param name="count">
    /// Number of elements.
    /// </param>
    /// <returns>
    /// The query with new added filter.
    /// </returns>
    public TQuery LimitToFirst(int count)
    {
        return LimitToFirst(() => count);
    }
}
