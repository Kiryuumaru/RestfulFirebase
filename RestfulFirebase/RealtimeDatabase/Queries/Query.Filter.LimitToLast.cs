using RestfulFirebase.Common.Abstractions;
using RestfulFirebase.RealtimeDatabase.References;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace RestfulFirebase.RealtimeDatabase.Queries;

public partial class FluentFilteredQuery<TQuery>
{
    /// <summary>
    /// Limits the result to last <paramref name="countFactory"/> items.
    /// </summary>
    /// <param name="countFactory">
    /// Number of elements.
    /// </param>
    /// <returns>
    /// The query with new added filter.
    /// </returns>
    public TQuery LimitToLast(Func<int> countFactory)
    {
        return FilterCore("limitToLast", () => countFactory());
    }

    /// <summary>
    /// Limits the result to last <paramref name="count"/> items.
    /// </summary>
    /// <param name="count">
    /// Number of elements.
    /// </param>
    /// <returns>
    /// The query with new added filter.
    /// </returns>
    public TQuery LimitToLast(int count)
    {
        return LimitToLast(() => count);
    }
}
