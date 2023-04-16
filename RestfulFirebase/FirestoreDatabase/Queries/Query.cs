using RestfulFirebase.Common.Abstractions;
using RestfulFirebase.Common.Utilities;
using RestfulFirebase.FirestoreDatabase.Models;
using RestfulFirebase.FirestoreDatabase.References;
using RestfulFirebase.FirestoreDatabase.Transactions;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace RestfulFirebase.FirestoreDatabase.Queries;

#region Root

/// <summary>
/// Runs a structured query.
/// </summary>
public abstract partial class QueryRoot : ICloneable<QueryRoot>
{
    /// <summary>
    /// Gets the collections to query.
    /// </summary>
    public IReadOnlyList<FromQuery> FromQuery { get; }

    /// <summary>
    /// gets the projection to return.
    /// </summary>
    public IReadOnlyList<SelectQuery> SelectQuery { get; }

    /// <summary>
    /// Gets the filter to apply.
    /// </summary>
    public IReadOnlyList<FilterQuery> WhereQuery { get; }

    /// <summary>
    /// Gets the order to apply to the query results.
    /// </summary>
    public IReadOnlyList<OrderByQuery> OrderByQuery { get; }

    /// <summary>
    /// Gets the order to apply to the query results.
    /// </summary>
    public IReadOnlyList<CursorQuery> StartCursorQuery { get; }

    /// <summary>
    /// Gets the order to apply to the query results.
    /// </summary>
    public IReadOnlyList<CursorQuery> EndCursorQuery { get; }

    /// <summary>
    /// Gets the list of cache <see cref="Document"/>.
    /// </summary>
    public IReadOnlyList<Document> CacheDocuments { get; }

    /// <summary>
    /// Gets <c>true</c> whether the position is on the given start values, relative to the sort order defined by the query; otherwise, <c>false</c> to skip the given start values. Default is <c>false</c>.
    /// </summary>
    public bool IsStartAfter { get; internal set; } = false;

    /// <summary>
    /// Gets <c>true</c> whether the position is on the given end values, relative to the sort order defined by the query; otherwise, <c>false</c> to skip the given end values. Default is <c>false</c>.
    /// </summary>
    public bool IsEndBefore { get; internal set; } = false;

    /// <summary>
    /// Gets the requested page size of pager async enumerator. Must be >= 1 if specified. Default is 20.
    /// </summary>
    public int SizeOfPages { get; internal set; } = 20;

    /// <summary>
    /// Gets the page to skip of pager async enumerator. Must be >= 0 if specified. Default is 0.
    /// </summary>
    public int PagesToSkip { get; internal set; } = 0;

    /// <summary>
    /// The <see cref="Transaction"/> to optionally perform an atomic operation.
    /// </summary>
    public Transaction? TransactionUsed { get; internal set; }

    /// <summary>
    /// Gets the authorization used for the operation.
    /// </summary>
    public IAuthorization? AuthorizationUsed { get; internal set; }

    /// <summary>
    /// Gets the document reference to run this query.
    /// </summary>
    public DocumentReference? DocumentReference { get; }

    /// <summary>
    /// Gets the type of the model to query.
    /// </summary>
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
    public Type? ModelType { get; }

    /// <summary>
    /// Gets the <see cref="FirebaseApp"/> used.
    /// </summary>
    internal FirebaseApp App { get; }

    internal readonly List<FromQuery> WritableFromQuery;
    internal readonly List<SelectQuery> WritableSelectQuery;
    internal readonly List<FilterQuery> WritableWhereQuery;
    internal readonly List<OrderByQuery> WritableOrderByQuery;
    internal readonly List<CursorQuery> WritableStartCursorQuery;
    internal readonly List<CursorQuery> WritableEndCursorQuery;
    internal readonly List<Document> WritableCacheDocuments;

    internal QueryRoot(FirebaseApp app, Type? modelType, DocumentReference? documentReference)
    {
        App = app;
        ModelType = modelType;
        DocumentReference = documentReference;

        WritableFromQuery = new();
        WritableSelectQuery = new();
        WritableWhereQuery = new();
        WritableOrderByQuery = new();
        WritableStartCursorQuery = new();
        WritableEndCursorQuery = new();
        WritableCacheDocuments = new();

        FromQuery = WritableFromQuery.AsReadOnly();
        SelectQuery = WritableSelectQuery.AsReadOnly();
        WhereQuery = WritableWhereQuery.AsReadOnly();
        OrderByQuery = WritableOrderByQuery.AsReadOnly();
        StartCursorQuery = WritableStartCursorQuery.AsReadOnly();
        EndCursorQuery = WritableEndCursorQuery.AsReadOnly();
        CacheDocuments = WritableCacheDocuments.AsReadOnly();
    }

    internal QueryRoot(QueryRoot query)
    {
        App = query.App;
        ModelType = query.ModelType;
        DocumentReference = query.DocumentReference;
        AuthorizationUsed = query.AuthorizationUsed;
        TransactionUsed = query.TransactionUsed;

        IsStartAfter = query.IsStartAfter;
        IsEndBefore = query.IsEndBefore;
        SizeOfPages = query.SizeOfPages;
        PagesToSkip = query.PagesToSkip;

        WritableFromQuery = new(query.WritableFromQuery);
        WritableSelectQuery = new(query.WritableSelectQuery);
        WritableWhereQuery = new(query.WritableWhereQuery);
        WritableOrderByQuery = new(query.WritableOrderByQuery);
        WritableStartCursorQuery = new(query.WritableStartCursorQuery);
        WritableEndCursorQuery = new(query.WritableEndCursorQuery);
        WritableCacheDocuments = new(query.WritableCacheDocuments);

        FromQuery = WritableFromQuery.AsReadOnly();
        SelectQuery = WritableSelectQuery.AsReadOnly();
        WhereQuery = WritableWhereQuery.AsReadOnly();
        OrderByQuery = WritableOrderByQuery.AsReadOnly();
        StartCursorQuery = WritableStartCursorQuery.AsReadOnly();
        EndCursorQuery = WritableEndCursorQuery.AsReadOnly();
        CacheDocuments = WritableCacheDocuments.AsReadOnly();
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
    internal FluentQuery(FirebaseApp app, Type? modelType, DocumentReference? documentReference)
        : base(app, modelType, documentReference)
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
    internal FluentQuery(FirebaseApp app, DocumentReference? documentReference)
        : base(app, typeof(TModel), documentReference)
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
    internal Query(FirebaseApp app, Type? modelType, DocumentReference? documentReference)
        : base(app, modelType, documentReference)
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
/// <typeparam name="TModel">
/// The type of the document model.
/// </typeparam>
public class Query<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TModel> : FluentQuery<Query<TModel>, TModel>, ICloneable<Query<TModel>>
    where TModel : class
{
    internal Query(FirebaseApp app, DocumentReference? documentReference)
        : base(app, documentReference)
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

internal class StructuredQuery
{
    public QueryRoot Query { get; }

    public List<StructuredFrom> From { get; }

    public List<StructuredSelect> Select { get; }

    public List<StructuredFilter> Where { get; }

    public List<StructuredOrderBy> OrderBy { get; }

    public List<StructuredCursor> StartCursor { get; }

    public List<StructuredCursor> EndCursor { get; }

    public List<Document> Cache { get; }

    public bool IsStartAfter { get; internal set; } = false;

    public bool IsEndBefore { get; internal set; } = false;

    public int SizeOfPages { get; internal set; } = 20;

    public int PagesToSkip { get; internal set; } = 0;

    public Transaction? TransactionUsed { get; internal set; }

    public IAuthorization? AuthorizationUsed { get; internal set; }

    public DocumentReference? DocumentReference { get; }

    public FirebaseApp App { get; }

    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
    public Type? ModelType { get; }

    public StructuredQuery(QueryRoot query)
    {
        Query = query;

        From = new();
        Select = new();
        Where = new();
        OrderBy = new();
        StartCursor = new();
        EndCursor = new();

        Cache = new(query.CacheDocuments);
        IsStartAfter = query.IsStartAfter;
        IsEndBefore = query.IsEndBefore;
        SizeOfPages = query.SizeOfPages;
        PagesToSkip = query.PagesToSkip;

        App = query.App;
        ModelType = query.ModelType;
        DocumentReference = query.DocumentReference;
        AuthorizationUsed = query.AuthorizationUsed;
        TransactionUsed = query.TransactionUsed;
    }

    public StructuredQuery(StructuredQuery query)
    {
        Query = query.Query;

        From = new(query.From);
        Select = new(query.Select);
        Where = new(query.Where);
        OrderBy = new(query.OrderBy);
        StartCursor = new(query.StartCursor);
        EndCursor = new(query.EndCursor);

        Cache = new(query.Cache);
        IsStartAfter = query.IsStartAfter;
        IsEndBefore = query.IsEndBefore;
        SizeOfPages = query.SizeOfPages;
        PagesToSkip = query.PagesToSkip;

        App = query.App;
        ModelType = query.ModelType;
        DocumentReference = query.DocumentReference;
        AuthorizationUsed = query.AuthorizationUsed;
        TransactionUsed = query.TransactionUsed;
    }
}

#endregion
