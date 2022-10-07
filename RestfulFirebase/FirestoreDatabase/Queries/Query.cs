﻿using RestfulFirebase.FirestoreDatabase.Enums;
using RestfulFirebase.FirestoreDatabase.References;
using RestfulFirebase.FirestoreDatabase.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.Json;

namespace RestfulFirebase.FirestoreDatabase.Queries;

/// <summary>
/// Runs a structured query.
/// </summary>
public abstract partial class BaseQuery<TQuery>
    where TQuery : BaseQuery<TQuery>
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
    /// Gets <c>true</c> whether the position is on the given start values, relative to the sort order defined by the query; otherwise, <c>false</c> to skip the given start values. Default is <c>false</c>.
    /// </summary>
    public bool IsStartAfter { get; private set; } = false;

    /// <summary>
    /// Gets <c>true</c> whether the position is on the given end values, relative to the sort order defined by the query; otherwise, <c>false</c> to skip the given end values. Default is <c>false</c>.
    /// </summary>
    public bool IsEndBefore { get; private set; } = false;

    /// <summary>
    /// Gets the requested page size of pager async enumerator. Must be >= 1 if specified. Default is 20.
    /// </summary>
    public int PageSize { get; private set; } = 20;

    /// <summary>
    /// Gets the page to skip of pager async enumerator. Must be >= 1 if specified. Default is 0.
    /// </summary>
    public int SkipPage { get; private set; } = 0;

    /// <summary>
    /// Gets <c>true</c> whether the "select" query is set to return the document name only; otherwise, <c>false</c>. Default is <c>false</c>.
    /// </summary>
    public bool IsSelectNameOnly { get; private set; } = false;

    /// <summary>
    /// Gets the document reference to run this query.
    /// </summary>
    public DocumentReference? DocumentReference { get; }

    /// <summary>
    /// Gets the <see cref="FirebaseApp"/> used.
    /// </summary>
    public FirebaseApp App { get; }

    /// <summary>
    /// Gets the type of the model to query.
    /// </summary>
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
    public Type? ModelType { get; }

    private readonly List<FromQuery> fromQuery;
    private readonly List<SelectQuery> selectQuery;
    private readonly List<FilterQuery> whereQuery;
    private readonly List<OrderByQuery> orderByQuery;
    private readonly List<CursorQuery> startCursorQuery;
    private readonly List<CursorQuery> endCursorQuery;

    internal BaseQuery(FirebaseApp app, Type? modelType, DocumentReference? documentReference)
    {
        App = app;
        ModelType = modelType;
        DocumentReference = documentReference;

        fromQuery = new();
        selectQuery = new();
        whereQuery = new();
        orderByQuery = new();
        startCursorQuery = new();
        endCursorQuery = new();

        FromQuery = fromQuery.AsReadOnly();
        SelectQuery = selectQuery.AsReadOnly();
        WhereQuery = whereQuery.AsReadOnly();
        OrderByQuery = orderByQuery.AsReadOnly();
        StartCursorQuery = startCursorQuery.AsReadOnly();
        EndCursorQuery = endCursorQuery.AsReadOnly();
    }
}

/// <summary>
/// Runs a structured query.
/// </summary>
public partial class Query : BaseQuery<Query>
{
    internal Query(FirebaseApp app, Type? modelType, DocumentReference? documentReference)
        : base(app, modelType, documentReference)
    {

    }
}

/// <summary>
/// Runs a structured query.
/// </summary>
/// <typeparam name="TModel">
/// The type of the document model.
/// </typeparam>
public partial class Query<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TModel> : BaseQuery<Query<TModel>>
    where TModel : class
{
    internal Query(FirebaseApp app, DocumentReference? documentReference)
        : base(app, typeof(TModel), documentReference)
    {

    }
}