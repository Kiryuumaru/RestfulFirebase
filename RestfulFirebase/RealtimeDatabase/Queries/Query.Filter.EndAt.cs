using RestfulFirebase.Common.Abstractions;
using RestfulFirebase.Common.Http;
using RestfulFirebase.RealtimeDatabase.References;
using System;
using System.Globalization;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace RestfulFirebase.RealtimeDatabase.Queries;

public partial class FluentFilteredQuery<TQuery>
{
    internal TQuery EndAtCore(Func<object?> valueFactory)
    {
        return FilterCore("endAt", valueFactory);
    }

    /// <summary>
    /// Instructs firebase to send data lower or equal to the <paramref name="valueFactory"/>. This must be preceded by an OrderBy query.
    /// </summary>
    /// <param name="valueFactory">
    /// Value to end at.
    /// </param>
    /// <returns>
    /// The query with new added filter.
    /// </returns>
    public TQuery EndAt(Func<string?> valueFactory)
    {
        return EndAtCore(() => valueFactory());
    }

    /// <summary>
    /// Instructs firebase to send data lower or equal to the <paramref name="valueFactory"/>. This must be preceded by an OrderBy query.
    /// </summary>
    /// <param name="valueFactory">
    /// Value to end at.
    /// </param>
    /// <returns>
    /// The query with new added filter.
    /// </returns>
    public TQuery EndAt(Func<double> valueFactory)
    {
        return EndAtCore(() => valueFactory());
    }

    /// <summary>
    /// Instructs firebase to send data lower or equal to the <paramref name="valueFactory"/>. This must be preceded by an OrderBy query.
    /// </summary>
    /// <param name="valueFactory">
    /// Value to end at.
    /// </param>
    /// <returns>
    /// The query with new added filter.
    /// </returns>
    public TQuery EndAt(Func<long> valueFactory)
    {
        return EndAtCore(() => valueFactory());
    }

    /// <summary>
    /// Instructs firebase to send data lower or equal to the <paramref name="value"/>. This must be preceded by an OrderBy query.
    /// </summary>
    /// <param name="value">
    /// Value to end at.
    /// </param>
    /// <returns>
    /// The query with new added filter.
    /// </returns>
    public TQuery EndAt(string? value)
    {
        return EndAtCore(() => value);
    }

    /// <summary>
    /// Instructs firebase to send data lower or equal to the <paramref name="value"/>. This must be preceded by an OrderBy query.
    /// </summary>
    /// <param name="value">
    /// Value to end at.
    /// </param>
    /// <returns>
    /// The query with new added filter.
    /// </returns>
    public TQuery EndAt(double value)
    {
        return EndAtCore(() => value);
    }

    /// <summary>
    /// Instructs firebase to send data lower or equal to the <paramref name="value"/>. This must be preceded by an OrderBy query.
    /// </summary>
    /// <param name="value">
    /// Value to end at.
    /// </param>
    /// <returns>
    /// The query with new added filter.
    /// </returns>
    public TQuery EndAt(long value)
    {
        return EndAtCore(() => value);
    }
}
