using RestfulFirebase.Common.Abstractions;
using RestfulFirebase.Common.Http;
using RestfulFirebase.RealtimeDatabase.References;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace RestfulFirebase.RealtimeDatabase.Queries;

public partial class FluentFilteredQuery<TQuery>
{
    /// <summary>
    /// Instructs firebase to send data equal to the <paramref name="valueFactory"/>. This must be preceded by an OrderBy query.
    /// </summary>
    /// <param name="valueFactory">
    /// Value to equal to.
    /// </param>
    /// <returns>
    /// The query with new added filter.
    /// </returns>
    public TQuery EqualTo(Func<string?> valueFactory)
    {
        return FilterCore("equalTo", () => valueFactory());
    }

    /// <summary>
    /// Instructs firebase to send data equal to the <paramref name="valueFactory"/>. This must be preceded by an OrderBy query.
    /// </summary>
    /// <param name="valueFactory">
    /// Value to equal to.
    /// </param>
    /// <returns>
    /// The query with new added filter.
    /// </returns>
    public TQuery EqualTo(Func<double> valueFactory)
    {
        return FilterCore("equalTo", () => valueFactory());
    }

    /// <summary>
    /// Instructs firebase to send data equal to the <paramref name="valueFactory"/>. This must be preceded by an OrderBy query.
    /// </summary>
    /// <param name="valueFactory">
    /// Value to equal to.
    /// </param>
    /// <returns>
    /// The query with new added filter.
    /// </returns>
    public TQuery EqualTo(Func<long> valueFactory)
    {
        return FilterCore("equalTo", () => valueFactory());
    }

    /// <summary>
    /// Instructs firebase to send data equal to the <paramref name="valueFactory"/>. This must be preceded by an OrderBy query.
    /// </summary>
    /// <param name="valueFactory">
    /// Value to equal to.
    /// </param>
    /// <returns>
    /// The query with new added filter.
    /// </returns>
    public TQuery EqualTo(Func<bool> valueFactory)
    {
        return FilterCore("equalTo", () => valueFactory());
    }

    /// <summary>
    /// Instructs firebase to send data equal to the <paramref name="value"/>. This must be preceded by an OrderBy query.
    /// </summary>
    /// <param name="value">
    /// Value to equal to.
    /// </param>
    /// <returns>
    /// The query with new added filter.
    /// </returns>
    public TQuery EqualTo(string value)
    {
        return EqualTo(() => value);
    }

    /// <summary>
    /// Instructs firebase to send data equal to the <paramref name="value"/>. This must be preceded by an OrderBy query.
    /// </summary>
    /// <param name="value">
    /// Value to equal to.
    /// </param>
    /// <returns>
    /// The query with new added filter.
    /// </returns>
    public TQuery EqualTo(double value)
    {
        return EqualTo(() => value);
    }

    /// <summary>
    /// Instructs firebase to send data equal to the <paramref name="value"/>. This must be preceded by an OrderBy query.
    /// </summary>
    /// <param name="value">
    /// Value to equal to.
    /// </param>
    /// <returns>
    /// The query with new added filter.
    /// </returns>
    public TQuery EqualTo(long value)
    {
        return EqualTo(() => value);
    }

    /// <summary>
    /// Instructs firebase to send data equal to the <paramref name="value"/>. This must be preceded by an OrderBy query.
    /// </summary>
    /// <param name="value">
    /// Value to equal to.
    /// </param>
    /// <returns>
    /// The query with new added filter.
    /// </returns>
    public TQuery EqualTo(bool value)
    {
        return EqualTo(() => value);
    }

    /// <summary>
    /// Instructs firebase to send data equal to <c>null</c>. This must be preceded by an OrderBy query.
    /// </summary>
    /// <returns>
    /// The query with new added filter.
    /// </returns>
    public TQuery EqualTo()
    {
        return EqualTo(() => default(string));
    }
}
