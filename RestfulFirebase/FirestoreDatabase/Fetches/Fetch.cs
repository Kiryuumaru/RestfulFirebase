using RestfulFirebase.Common.Abstractions;
using RestfulFirebase.FirestoreDatabase.Models;
using RestfulFirebase.FirestoreDatabase.References;
using RestfulFirebase.FirestoreDatabase.Transactions;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace RestfulFirebase.FirestoreDatabase.Fetches;

/// <summary>
/// Runs a fetch or get operation.
/// </summary>
public abstract partial class Fetch : FluentRequest
{
    /// <summary>
    /// Gets the list of document references to fetch.
    /// </summary>
    public IReadOnlyList<DocumentReference> DocumentReferences { get; }

    /// <summary>
    /// Gets the list of document to fetch.
    /// </summary>
    public IReadOnlyList<Document> Documents { get; }

    /// <summary>
    /// Gets the list of cache <see cref="Document"/>.
    /// </summary>
    public IReadOnlyList<Document> CacheDocuments { get; }

    /// <summary>
    /// The <see cref="Transaction"/> to optionally perform an atomic operation.
    /// </summary>
    public Transaction? TransactionUsed { get; internal set; }

    /// <summary>
    /// Gets the authorization used for the operation.
    /// </summary>
    public IAuthorization? AuthorizationUsed { get; internal set; }

    /// <summary>
    /// Gets the type of the model to fetch.
    /// </summary>
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
    public Type? ModelType { get; }

    internal readonly List<DocumentReference> WritableDocumentReferences;
    internal readonly List<Document> WritableDocuments;
    internal readonly List<Document> WritableCacheDocuments;

    internal Fetch(FirebaseApp app, Type? modelType)
        : base(app)
    {
        ModelType = modelType;

        WritableDocumentReferences = new();
        WritableDocuments = new();
        WritableCacheDocuments = new();

        DocumentReferences = WritableDocumentReferences.AsReadOnly();
        Documents = WritableDocuments.AsReadOnly();
        CacheDocuments = WritableCacheDocuments.AsReadOnly();
    }

    internal Fetch(Fetch fetch)
        : base(fetch.App)
    {
        ModelType = fetch.ModelType;
        AuthorizationUsed = fetch.AuthorizationUsed;
        TransactionUsed = fetch.TransactionUsed;

        WritableDocumentReferences = fetch.WritableDocumentReferences;
        WritableDocuments = fetch.WritableDocuments;
        WritableCacheDocuments = fetch.WritableCacheDocuments;

        DocumentReferences = fetch.DocumentReferences;
        Documents = fetch.Documents;
        CacheDocuments = fetch.CacheDocuments;
    }
}

/// <summary>
/// Runs a structured fetch.
/// </summary>
public abstract partial class FluentFetchRoot<TFetch> : Fetch
    where TFetch : FluentFetchRoot<TFetch>
{
    internal FluentFetchRoot(FirebaseApp app, Type? modelType)
        : base(app, modelType)
    {

    }

    internal FluentFetchRoot(Fetch fetch)
        : base(fetch)
    {

    }
}

/// <summary>
/// Runs a structured fetch.
/// </summary>
public abstract partial class FluentFetchRoot<TFetch, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TModel> : FluentFetchRoot<TFetch>
    where TFetch : FluentFetchRoot<TFetch, TModel>
    where TModel : class
{
    internal FluentFetchRoot(FirebaseApp app)
        : base(app, typeof(TModel))
    {

    }

    internal FluentFetchRoot(Fetch fetch)
        : base(fetch)
    {

    }
}

#region Instantiable

/// <summary>
/// Runs a structured fetch.
/// </summary>
public partial class FetchRoot : FluentFetchRoot<FetchRoot>
{
    internal FetchRoot(FirebaseApp app, Type? modelType)
        : base(app, modelType)
    {

    }

    internal FetchRoot(Fetch fetch)
        : base(fetch)
    {

    }
}

/// <summary>
/// Runs a structured fetch.
/// </summary>
/// <typeparam name="TModel">
/// The type of the document model.
/// </typeparam>
public partial class FetchRoot<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TModel> : FluentFetchRoot<FetchRoot<TModel>, TModel>
    where TModel : class
{
    internal FetchRoot(FirebaseApp app)
        : base(app)
    {

    }

    internal FetchRoot(Fetch fetch)
        : base(fetch)
    {

    }
}

#endregion
