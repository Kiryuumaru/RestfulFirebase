using RestfulFirebase.Common.Abstractions;
using RestfulFirebase.RealtimeDatabase.References;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace RestfulFirebase.RealtimeDatabase.Queries;

public partial class FluentFilteredQuery<TQuery>
{
    /// <summary>
    /// Instructs firebase to send data greater or equal to the <paramref name="valueFactory"/>. This must be preceded by an OrderBy query.
    /// </summary>
    /// <param name="valueFactory">
    /// Value to start at.
    /// </param>
    /// <returns>
    /// The query with new added filter.
    /// </returns>
    public TQuery StartAt(Func<string> valueFactory)
    {
        return FilterCore("startAt", () => valueFactory());
    }

    /// <summary>
    /// Instructs firebase to send data greater or equal to the <paramref name="valueFactory"/>. This must be preceded by an OrderBy query.
    /// </summary>
    /// <param name="valueFactory">
    /// Value to start at.
    /// </param>
    /// <returns>
    /// The query with new added filter.
    /// </returns>
    public TQuery StartAt(Func<double> valueFactory)
    {
        return FilterCore("startAt", () => valueFactory());
    }

    /// <summary>
    /// Instructs firebase to send data greater or equal to the <paramref name="valueFactory"/>. This must be preceded by an OrderBy query.
    /// </summary>
    /// <param name="valueFactory">
    /// Value to start at.
    /// </param>
    /// <returns>
    /// The query with new added filter.
    /// </returns>
    public TQuery StartAt(Func<long> valueFactory)
    {
        return FilterCore("startAt", () => valueFactory());
    }

    /// <summary>
    /// Instructs firebase to send data greater or equal to the <paramref name="value"/>. This must be preceded by an OrderBy query.
    /// </summary>
    /// <param name="value">
    /// Value to start at.
    /// </param>
    /// <returns>
    /// The query with new added filter.
    /// </returns>
    public TQuery StartAt(string value)
    {
        return StartAt(() => value);
    }

    /// <summary>
    /// Instructs firebase to send data greater or equal to the <paramref name="value"/>. This must be preceded by an OrderBy query.
    /// </summary>
    /// <param name="value">
    /// Value to start at.
    /// </param>
    /// <returns>
    /// The query with new added filter.
    /// </returns>
    public TQuery StartAt(double value)
    {
        return StartAt(() => value);
    }

    /// <summary>
    /// Instructs firebase to send data greater or equal to the <paramref name="value"/>. This must be preceded by an OrderBy query.
    /// </summary>
    /// <param name="value">
    /// Value to start at.
    /// </param>
    /// <returns>
    /// The query with new added filter.
    /// </returns>
    public TQuery StartAt(long value)
    {
        return StartAt(() => value);
    }
}
