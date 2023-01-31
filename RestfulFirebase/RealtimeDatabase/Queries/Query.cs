using RestfulFirebase.Common.Abstractions;
using RestfulHelpers.Common;
using RestfulFirebase.Common.Utilities;
using RestfulFirebase.RealtimeDatabase.References;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace RestfulFirebase.RealtimeDatabase.Queries;

/// <summary>
/// The base implementation for firebase realtime database query operations.
/// </summary>
public abstract partial class QueryRoot
{
    /// <summary>
    /// Gets the <see cref="References.Reference"/> used.
    /// </summary>
    public Reference Reference { get; }

    /// <summary>
    /// Gets the last <see cref="QueryRoot"/> of the fluent calls.
    /// </summary>
    public QueryRoot? LastQuery { get; }

    /// <summary>
    /// Gets the <see cref="FirebaseApp"/> used.
    /// </summary>
    internal FirebaseApp App { get; }

    internal Func<CancellationToken, ValueTask<HttpResponse<string>>> SegementFactory;
    internal HttpClient? Client;

    internal QueryRoot(Reference reference, QueryRoot? lastQuery, Func<CancellationToken, ValueTask<HttpResponse<string>>> segementFactory)
    {
        App = reference.App;
        Reference = reference;
        LastQuery = lastQuery;
        SegementFactory = segementFactory;
    }

    internal QueryRoot(QueryRoot query)
    {
        App = query.App;
        Reference = query.Reference;
        LastQuery = query.LastQuery;
        SegementFactory = query.SegementFactory;
    }
}

/// <summary>
/// Runs a structured query.
/// </summary>
public abstract partial class FluentQuery<TQuery> : QueryRoot
    where TQuery : FluentQuery<TQuery>
{
    internal FluentQuery(Reference reference, QueryRoot? lastQuery, Func<CancellationToken, ValueTask<HttpResponse<string>>> segementFactory)
        : base(reference, lastQuery, segementFactory)
    {

    }

    internal FluentQuery(QueryRoot query)
        : base(query)
    {

    }
}

/// <summary>
/// Runs a structured query.
/// </summary>
public abstract partial class FluentOrderedQuery<TQuery> : FluentQuery<TQuery>
    where TQuery : FluentOrderedQuery<TQuery>
{
    internal FluentOrderedQuery(Reference reference, QueryRoot? lastQuery, Func<CancellationToken, ValueTask<HttpResponse<string>>> segementFactory)
        : base(reference, lastQuery, segementFactory)
    {

    }

    internal FluentOrderedQuery(QueryRoot query)
        : base(query)
    {

    }
}

/// <summary>
/// Runs a structured query.
/// </summary>
public abstract partial class FluentFilteredQuery<TQuery> : FluentQuery<TQuery>
    where TQuery : FluentFilteredQuery<TQuery>
{
    internal FluentFilteredQuery(Reference reference, QueryRoot? lastQuery, Func<CancellationToken, ValueTask<HttpResponse<string>>> segementFactory)
        : base(reference, lastQuery, segementFactory)
    {

    }

    internal FluentFilteredQuery(QueryRoot query)
        : base(query)
    {

    }
}

/// <summary>
/// Runs a structured query.
/// </summary>
public class Query : FluentQuery<Query>
{
    internal Query(Reference reference, QueryRoot? lastQuery, Func<CancellationToken, ValueTask<HttpResponse<string>>> segementFactory)
        : base(reference, lastQuery, segementFactory)
    {

    }

    internal Query(QueryRoot query)
        : base(query)
    {

    }
}

/// <summary>
/// Runs a structured query.
/// </summary>
public class OrderedQuery : FluentOrderedQuery<OrderedQuery>
{
    internal OrderedQuery(Reference reference, QueryRoot? lastQuery, Func<CancellationToken, ValueTask<HttpResponse<string>>> segementFactory)
        : base(reference, lastQuery, segementFactory)
    {

    }

    internal OrderedQuery(QueryRoot query)
        : base(query)
    {

    }
}

/// <summary>
/// Runs a structured query.
/// </summary>
public class FilteredQuery : FluentFilteredQuery<FilteredQuery>
{
    internal FilteredQuery(Reference reference, QueryRoot? lastQuery, Func<CancellationToken, ValueTask<HttpResponse<string>>> segementFactory)
        : base(reference, lastQuery, segementFactory)
    {

    }

    internal FilteredQuery(QueryRoot query)
        : base(query)
    {

    }
}
