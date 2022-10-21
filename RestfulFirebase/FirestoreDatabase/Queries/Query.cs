using RestfulFirebase.Common.Abstractions;
using RestfulFirebase.FirestoreDatabase.Models;
using RestfulFirebase.FirestoreDatabase.References;
using RestfulFirebase.FirestoreDatabase.Transactions;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace RestfulFirebase.FirestoreDatabase.Queries;

/// <summary>
/// Runs a structured query.
/// </summary>
public abstract partial class Query
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
    public FirebaseApp App { get; }

    internal readonly List<FromQuery> WritableFromQuery;
    internal readonly List<SelectQuery> WritableSelectQuery;
    internal readonly List<FilterQuery> WritableWhereQuery;
    internal readonly List<OrderByQuery> WritableOrderByQuery;
    internal readonly List<CursorQuery> WritableStartCursorQuery;
    internal readonly List<CursorQuery> WritableEndCursorQuery;
    internal readonly List<Document> WritableCacheDocuments;

    internal Query(FirebaseApp app, Type? modelType, DocumentReference? documentReference)
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

    internal Query(Query query)
    {
        App = query.App;
        ModelType = query.ModelType;
        DocumentReference = query.DocumentReference;
        AuthorizationUsed = query.AuthorizationUsed;
        TransactionUsed = query.TransactionUsed;

        WritableFromQuery = query.WritableFromQuery;
        WritableSelectQuery = query.WritableSelectQuery;
        WritableWhereQuery = query.WritableWhereQuery;
        WritableOrderByQuery = query.WritableOrderByQuery;
        WritableStartCursorQuery = query.WritableStartCursorQuery;
        WritableEndCursorQuery = query.WritableEndCursorQuery;
        WritableCacheDocuments = query.WritableCacheDocuments;

        FromQuery = query.FromQuery;
        SelectQuery = query.SelectQuery;
        WhereQuery = query.WhereQuery;
        OrderByQuery = query.OrderByQuery;
        StartCursorQuery = query.StartCursorQuery;
        EndCursorQuery = query.EndCursorQuery;
        CacheDocuments = query.CacheDocuments;

        IsStartAfter = query.IsStartAfter;
        IsEndBefore = query.IsEndBefore;
        SizeOfPages = query.SizeOfPages;
        PagesToSkip = query.PagesToSkip;
    }
}

/// <summary>
/// Runs a structured query.
/// </summary>
public abstract partial class FluentQueryRoot<TQuery> : Query
    where TQuery : FluentQueryRoot<TQuery>
{
    internal FluentQueryRoot(FirebaseApp app, Type? modelType, DocumentReference? documentReference)
        : base(app, modelType, documentReference)
    {

    }

    internal FluentQueryRoot(Query query)
        : base(query)
    {

    }
}

/// <summary>
/// Runs a structured query.
/// </summary>
public abstract partial class FluentQueryRoot<TQuery, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TModel> : FluentQueryRoot<TQuery>
    where TQuery : FluentQueryRoot<TQuery, TModel>
    where TModel : class
{
    internal FluentQueryRoot(FirebaseApp app, DocumentReference? documentReference)
        : base(app, typeof(TModel), documentReference)
    {

    }

    internal FluentQueryRoot(Query query)
        : base(query)
    {

    }
}

#region Instantiable

/// <summary>
/// Runs a structured query.
/// </summary>
public class QueryRoot : FluentQueryRoot<QueryRoot>
{
    internal QueryRoot(FirebaseApp app, Type? modelType, DocumentReference? documentReference)
        : base(app, modelType, documentReference)
    {

    }

    internal QueryRoot(Query query)
        : base(query)
    {

    }
}

/// <summary>
/// Runs a structured query.
/// </summary>
/// <typeparam name="TModel">
/// The type of the document model.
/// </typeparam>
public class QueryRoot<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TModel> : FluentQueryRoot<QueryRoot<TModel>, TModel>
    where TModel : class
{
    internal QueryRoot(FirebaseApp app, DocumentReference? documentReference)
        : base(app, documentReference)
    {

    }

    internal QueryRoot(Query query)
        : base(query)
    {

    }
}

internal class StructuredQuery
{
    public Query Query { get; }

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

    public StructuredQuery(Query query)
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
