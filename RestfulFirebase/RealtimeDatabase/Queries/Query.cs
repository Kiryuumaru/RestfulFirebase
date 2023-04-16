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

#region Root

/// <summary>
/// The base implementation for firebase realtime database query operations.
/// </summary>
public abstract partial class QueryRoot : ICloneable<QueryRoot>
{
    /// <summary>
    /// Gets the filter to apply.
    /// </summary>
    public IReadOnlyList<FilterQuery> FilterQuery { get; }

    /// <summary>
    /// Gets the order by to apply.
    /// </summary>
    public IReadOnlyList<OrderByQuery> OrderByQuery { get; }

    /// <summary>
    /// Gets the authorization used for the operation.
    /// </summary>
    public IAuthorization? AuthorizationUsed { get; internal set; }

    /// <summary>
    /// Gets the reference to run this query.
    /// </summary>
    public Reference Reference { get; }

    /// <summary>
    /// Gets the type of the model to query.
    /// </summary>
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
    public Type? ModelType { get; }

    /// <summary>
    /// Gets the <see cref="FirebaseApp"/> used.
    /// </summary>
    internal FirebaseApp App { get; }

    internal readonly List<FilterQuery> WritableFilterQuery;
    internal readonly List<OrderByQuery> WritableOrderByQuery;

    internal QueryRoot(Type? modelType, Reference reference)
    {
        App = reference.App;
        ModelType = modelType;
        Reference = reference;

        WritableFilterQuery = new();
        WritableOrderByQuery = new();

        FilterQuery = WritableFilterQuery.AsReadOnly();
        OrderByQuery = WritableOrderByQuery.AsReadOnly();
    }

    internal QueryRoot(QueryRoot query)
    {
        App = query.App;
        ModelType = query.ModelType;
        Reference = query.Reference;

        WritableFilterQuery = new(query.WritableFilterQuery);
        WritableOrderByQuery = new(query.WritableOrderByQuery);

        FilterQuery = WritableFilterQuery.AsReadOnly();
        OrderByQuery = WritableOrderByQuery.AsReadOnly();
    }

    /// <inheritdoc/>
    public QueryRoot Clone() => (QueryRoot)CoreClone();

    /// <inheritdoc/>
    object ICloneable.Clone() => CoreClone();

    /// <inheritdoc/>
    protected abstract object CoreClone();
}

/// <summary>
/// Runs a structured query.
/// </summary>
public abstract partial class FluentQuery<TQuery> : QueryRoot, ICloneable<FluentQuery<TQuery>>
    where TQuery : FluentQuery<TQuery>
{
    internal FluentQuery(Type? modelType, Reference reference)
        : base(modelType, reference)
    {

    }

    internal FluentQuery(QueryRoot query)
        : base(query)
    {

    }

    /// <inheritdoc/>
    public new FluentQuery<TQuery> Clone() => (FluentQuery<TQuery>)CoreClone();
}

/// <summary>
/// Runs a structured query.
/// </summary>
public abstract partial class FluentQuery<TQuery, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TModel> : FluentQuery<TQuery>, ICloneable<FluentQuery<TQuery, TModel>>
    where TQuery : FluentQuery<TQuery, TModel>
    where TModel : class
{
    internal FluentQuery(Reference reference)
        : base(typeof(TModel), reference)
    {

    }

    internal FluentQuery(QueryRoot query)
        : base(query)
    {

    }

    /// <inheritdoc/>
    public new FluentQuery<TQuery, TModel> Clone() => (FluentQuery<TQuery, TModel>)CoreClone();
}

#endregion

#region Instantiable

/// <summary>
/// Runs a structured query.
/// </summary>
public class Query : FluentQuery<Query>, ICloneable<Query>
{
    internal Query(Type? modelType, Reference reference)
        : base(modelType, reference)
    {

    }

    internal Query(QueryRoot query)
        : base(query)
    {

    }

    /// <inheritdoc/>
    public new Query Clone() => (Query)CoreClone();

    /// <inheritdoc/>
    protected override object CoreClone() => new Query(this);
}

/// <summary>
/// Runs a structured query.
/// </summary>
public class Query<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TModel> : FluentQuery<Query<TModel>, TModel>, ICloneable<Query<TModel>>
    where TModel : class
{
    internal Query(Reference reference)
        : base(reference)
    {

    }

    internal Query(QueryRoot query)
        : base(query)
    {

    }

    /// <inheritdoc/>
    public new Query<TModel> Clone() => (Query<TModel>)CoreClone();

    /// <inheritdoc/>
    protected override object CoreClone() => new Query<TModel>(this);
}

#endregion
