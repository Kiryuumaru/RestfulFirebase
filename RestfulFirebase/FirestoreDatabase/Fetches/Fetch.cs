using RestfulFirebase.Common.Abstractions;
using RestfulFirebase.FirestoreDatabase.Models;
using RestfulFirebase.FirestoreDatabase.References;
using RestfulFirebase.FirestoreDatabase.Transactions;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace RestfulFirebase.FirestoreDatabase.Fetches;

/// <summary>
/// Runs a fetch or get operation.
/// </summary>
public abstract partial class Fetch : ICloneable<Fetch>
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

    /// <summary>
    /// Gets the <see cref="FirebaseApp"/> used.
    /// </summary>
    internal FirebaseApp App { get; }

    internal readonly List<DocumentReference> WritableDocumentReferences;
    internal readonly List<Document> WritableDocuments;
    internal readonly List<Document> WritableCacheDocuments;

    internal Fetch(FirebaseApp app, Type? modelType)
    {
        App = app;
        ModelType = modelType;

        WritableDocumentReferences = new();
        WritableDocuments = new();
        WritableCacheDocuments = new();

        DocumentReferences = WritableDocumentReferences.AsReadOnly();
        Documents = WritableDocuments.AsReadOnly();
        CacheDocuments = WritableCacheDocuments.AsReadOnly();
    }

    internal Fetch(Fetch fetch, bool isClone)
    {
        App = fetch.App;
        ModelType = fetch.ModelType;
        AuthorizationUsed = fetch.AuthorizationUsed;
        TransactionUsed = fetch.TransactionUsed;

        if (isClone)
        {
            WritableDocumentReferences = new (fetch.WritableDocumentReferences);
            WritableDocuments = new(fetch.WritableDocuments);
            WritableCacheDocuments = new(fetch.WritableCacheDocuments);

            DocumentReferences = WritableDocumentReferences.AsReadOnly();
            Documents = WritableDocuments.AsReadOnly();
            CacheDocuments = WritableCacheDocuments.AsReadOnly();
        }
        else
        {
            WritableDocumentReferences = fetch.WritableDocumentReferences;
            WritableDocuments = fetch.WritableDocuments;
            WritableCacheDocuments = fetch.WritableCacheDocuments;

            DocumentReferences = fetch.DocumentReferences;
            Documents = fetch.Documents;
            CacheDocuments = fetch.CacheDocuments;
        }
    }

    /// <inheritdoc/>
    public Fetch Clone() => (Fetch)CoreClone();

    /// <inheritdoc/>
    object ICloneable.Clone() => CoreClone();

    /// <inheritdoc/>
    protected abstract object CoreClone();
}

/// <summary>
/// Runs a structured fetch.
/// </summary>
public abstract partial class FluentFetchRoot<TFetch> : Fetch, ICloneable<FluentFetchRoot<TFetch>>
    where TFetch : FluentFetchRoot<TFetch>
{
    internal FluentFetchRoot(FirebaseApp app, Type? modelType)
        : base(app, modelType)
    {

    }

    internal FluentFetchRoot(Fetch fetch, bool isClone)
        : base(fetch, isClone)
    {

    }

    /// <inheritdoc/>
    public new FluentFetchRoot<TFetch> Clone() => (FluentFetchRoot<TFetch>)CoreClone();
}

/// <summary>
/// Runs a structured fetch.
/// </summary>
public abstract partial class FluentFetchRoot<TFetch, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TModel> : FluentFetchRoot<TFetch>, ICloneable<FluentFetchRoot<TFetch, TModel>>
    where TFetch : FluentFetchRoot<TFetch, TModel>
    where TModel : class
{
    internal FluentFetchRoot(FirebaseApp app)
        : base(app, typeof(TModel))
    {

    }

    internal FluentFetchRoot(Fetch fetch, bool isClone)
        : base(fetch, isClone)
    {

    }

    /// <inheritdoc/>
    public new FluentFetchRoot<TFetch, TModel> Clone() => (FluentFetchRoot<TFetch, TModel>)CoreClone();
}

#region Instantiable

/// <summary>
/// Runs a structured fetch.
/// </summary>
public class FetchRoot : FluentFetchRoot<FetchRoot>, ICloneable<FetchRoot>
{
    internal FetchRoot(FirebaseApp app, Type? modelType)
        : base(app, modelType)
    {

    }

    internal FetchRoot(Fetch fetch, bool isClone)
        : base(fetch, isClone)
    {

    }

    /// <inheritdoc/>
    public new FetchRoot Clone() => (FetchRoot)CoreClone();

    /// <inheritdoc/>
    protected override object CoreClone() => new FetchRoot(this, true);
}

/// <summary>
/// Runs a structured fetch.
/// </summary>
/// <typeparam name="TModel">
/// The type of the document model.
/// </typeparam>
public class FetchRoot<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TModel> : FluentFetchRoot<FetchRoot<TModel>, TModel>, ICloneable<FetchRoot<TModel>>
    where TModel : class
{
    internal FetchRoot(FirebaseApp app)
        : base(app)
    {

    }

    internal FetchRoot(Fetch fetch, bool isClone)
        : base(fetch, isClone)
    {

    }

    /// <inheritdoc/>
    public new FetchRoot<TModel> Clone() => (FetchRoot<TModel>)CoreClone();

    /// <inheritdoc/>
    protected override object CoreClone() => new FetchRoot<TModel>(this, true);
}

#endregion
