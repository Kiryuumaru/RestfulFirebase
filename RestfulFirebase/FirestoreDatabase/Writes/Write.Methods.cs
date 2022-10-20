using RestfulFirebase.FirestoreDatabase.References;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using System.Threading;
using RestfulFirebase.Common.Http;
using RestfulFirebase.FirestoreDatabase.Transactions;
using RestfulFirebase.Common.Abstractions;
using RestfulFirebase.FirestoreDatabase.Models;
using System.Reflection;
using System.Linq;

namespace RestfulFirebase.FirestoreDatabase.Writes;

public abstract partial class Write
{
    /// <summary>
    /// Runs the write operation.
    /// </summary>
    /// <param name="transaction">
    /// The <see cref="Transaction"/> to optionally perform an atomic operation.
    /// </param>
    /// <param name="authorization">
    /// The authorization used for the operation.
    /// </param>
    /// <param name="cancellationToken">
    /// The <see cref="CancellationToken"/> that propagates notification if the operations should be canceled.
    /// </param>
    /// <returns>
    /// The <see cref="Task"/> proxy that represents the <see cref="HttpResponse"/>.
    /// </returns>
    public async Task<HttpResponse> Run(
        Transaction? transaction = default,
        IAuthorization? authorization = default,
        CancellationToken cancellationToken = default)
    {
        HttpResponse response = new();

        if (PatchDocuments.Count != 0 ||
            DeleteDocuments.Count != 0 ||
            TransformDocuments.Count != 0)
        {
            var commitOperation = await App.FirestoreDatabase.ExecuteCommit(this, transaction, authorization, cancellationToken);
            response.Append(commitOperation);
            if (commitOperation.IsError)
            {
                return response;
            }
        }
        if (CreateDocuments.Count != 0)
        {
            var createOperation = await App.FirestoreDatabase.ExecuteCreate(this, null, transaction, authorization, cancellationToken);
            response.Append(createOperation);
            if (createOperation.IsError)
            {
                return response;
            }
        }

        return response;
    }

    /// <summary>
    /// Runs the write operation.
    /// </summary>
    /// <param name="cacheDocuments">
    /// The cache of documents to recycle if it matched its reference.
    /// </param>
    /// <param name="transaction">
    /// The <see cref="Transaction"/> to optionally perform an atomic operation.
    /// </param>
    /// <param name="authorization">
    /// The authorization used for the operation.
    /// </param>
    /// <param name="cancellationToken">
    /// The <see cref="CancellationToken"/> that propagates notification if the operations should be canceled.
    /// </param>
    /// <returns>
    /// The <see cref="Task"/> proxy that represents the <see cref="HttpResponse"/>.
    /// </returns>
    public async Task<HttpResponse<GetDocumentsResult>> RunAndGet(
        IEnumerable<Document>? cacheDocuments,
        Transaction? transaction = default,
        IAuthorization? authorization = default,
        CancellationToken cancellationToken = default)
    {
        HttpResponse response = new();
        List<DocumentTimestamp> found = new();
        List<DocumentReferenceTimestamp> missing = new();

        if (PatchDocuments.Count != 0 ||
            DeleteDocuments.Count != 0 ||
            TransformDocuments.Count != 0)
        {
            var commitOperation = await App.FirestoreDatabase.ExecuteCommit(this, transaction, authorization, cancellationToken);
            response.Append(commitOperation);
            if (commitOperation.IsError)
            {
                return new(response);
            }

            List<DocumentReference> docRefs = new();
            List<Document> docs = new();

            docs.AddRange(PatchDocuments);
            docRefs.AddRange(DeleteDocuments);
            docRefs.AddRange(TransformDocuments.Select(i => i.DocumentReference));

            var getDocumentResponse = await App.FirestoreDatabase.GetDocument(docRefs, docs, cacheDocuments, transaction, authorization, cancellationToken);
            response.Append(getDocumentResponse);
            if (getDocumentResponse.IsError)
            {
                return new(response);
            }
            if (getDocumentResponse.Result?.Found != null)
            {
                found.AddRange(getDocumentResponse.Result.Found);
            }
            if (getDocumentResponse.Result?.Missing != null)
            {
                missing.AddRange(getDocumentResponse.Result.Missing);
            }
        }
        if (CreateDocuments.Count != 0)
        {
            var createOperation = await App.FirestoreDatabase.ExecuteCreate(this, cacheDocuments, transaction, authorization, cancellationToken);
            response.Append(createOperation);
            if (createOperation.IsError)
            {
                return new(response);
            }
            if (createOperation.Result?.Found != null)
            {
                found.AddRange(createOperation.Result.Found);
            }
            if (createOperation.Result?.Missing != null)
            {
                missing.AddRange(createOperation.Result.Missing);
            }
        }

        return new(new(found.AsReadOnly(), missing.AsReadOnly()), response);
    }

    /// <summary>
    /// Runs the write operation.
    /// </summary>
    /// <param name="cacheDocuments">
    /// The cache of documents to recycle if it matched its reference.
    /// </param>
    /// <param name="transaction">
    /// The <see cref="Transaction"/> to optionally perform an atomic operation.
    /// </param>
    /// <param name="authorization">
    /// The authorization used for the operation.
    /// </param>
    /// <param name="cancellationToken">
    /// The <see cref="CancellationToken"/> that propagates notification if the operations should be canceled.
    /// </param>
    /// <returns>
    /// The <see cref="Task"/> proxy that represents the <see cref="HttpResponse"/>.
    /// </returns>
    public async Task<HttpResponse<GetDocumentsResult<TModel>>> RunAndGet<TModel>(
        IEnumerable<Document>? cacheDocuments,
        Transaction? transaction = default,
        IAuthorization? authorization = default,
        CancellationToken cancellationToken = default)
        where TModel : class
    {
        HttpResponse response = new();
        List<DocumentTimestamp<TModel>> found = new();
        List<DocumentReferenceTimestamp> missing = new();

        if (PatchDocuments.Count != 0 ||
            DeleteDocuments.Count != 0 ||
            TransformDocuments.Count != 0)
        {
            var commitOperation = await App.FirestoreDatabase.ExecuteCommit(this, transaction, authorization, cancellationToken);
            response.Append(commitOperation);
            if (commitOperation.IsError)
            {
                return new(response);
            }

            List<DocumentReference> docRefs = new();
            List<Document> docs = new();
            List<Document<TModel>> typedDocs = new();

            docs.AddRange(PatchDocuments);
            docRefs.AddRange(DeleteDocuments);
            docRefs.AddRange(TransformDocuments.Select(i => i.DocumentReference));

            var getDocumentResponse = await App.FirestoreDatabase.GetDocument(docRefs, docs, typedDocs, cacheDocuments, transaction, authorization, cancellationToken);
            response.Append(getDocumentResponse);
            if (getDocumentResponse.IsError)
            {
                return new(response);
            }
            if (getDocumentResponse.Result?.Found != null)
            {
                found.AddRange(getDocumentResponse.Result.Found);
            }
            if (getDocumentResponse.Result?.Missing != null)
            {
                missing.AddRange(getDocumentResponse.Result.Missing);
            }
        }
        if (CreateDocuments.Count != 0)
        {
            var createOperation = await App.FirestoreDatabase.ExecuteCreate<TModel>(this, cacheDocuments, transaction, authorization, cancellationToken);
            response.Append(createOperation);
            if (createOperation.IsError)
            {
                return new(response);
            }
            if (createOperation.Result?.Found != null)
            {
                found.AddRange(createOperation.Result.Found);
            }
            if (createOperation.Result?.Missing != null)
            {
                missing.AddRange(createOperation.Result.Missing);
            }
        }

        return new(new(found.AsReadOnly(), missing.AsReadOnly()), response);
    }

    /// <summary>
    /// Runs the write operation.
    /// </summary>
    /// <param name="transaction">
    /// The <see cref="Transaction"/> to optionally perform an atomic operation.
    /// </param>
    /// <param name="authorization">
    /// The authorization used for the operation.
    /// </param>
    /// <param name="cancellationToken">
    /// The <see cref="CancellationToken"/> that propagates notification if the operations should be canceled.
    /// </param>
    /// <returns>
    /// The <see cref="Task"/> proxy that represents the <see cref="HttpResponse"/>.
    /// </returns>
    public virtual Task<HttpResponse<GetDocumentsResult>> RunAndGet(
        Transaction? transaction = default,
        IAuthorization? authorization = default,
        CancellationToken cancellationToken = default)
    {
        return RunAndGet(null, transaction, authorization, cancellationToken);
    }

    /// <summary>
    /// Runs the write operation.
    /// </summary>
    /// <param name="transaction">
    /// The <see cref="Transaction"/> to optionally perform an atomic operation.
    /// </param>
    /// <param name="authorization">
    /// The authorization used for the operation.
    /// </param>
    /// <param name="cancellationToken">
    /// The <see cref="CancellationToken"/> that propagates notification if the operations should be canceled.
    /// </param>
    /// <returns>
    /// The <see cref="Task"/> proxy that represents the <see cref="HttpResponse"/>.
    /// </returns>
    public Task<HttpResponse<GetDocumentsResult<TModel>>> RunAndGet<TModel>(
        Transaction? transaction = default,
        IAuthorization? authorization = default,
        CancellationToken cancellationToken = default)
        where TModel : class
    {
        return RunAndGet<TModel>(null, transaction, authorization, cancellationToken);
    }
}

public partial class FluentWriteWithDocumentTransform<TWrite, TModel>
{
    /// <summary>
    /// Runs the write operation.
    /// </summary>
    /// <param name="cacheDocuments">
    /// The cache of documents to recycle if it matched its reference.
    /// </param>
    /// <param name="transaction">
    /// The <see cref="Transaction"/> to optionally perform an atomic operation.
    /// </param>
    /// <param name="authorization">
    /// The authorization used for the operation.
    /// </param>
    /// <param name="cancellationToken">
    /// The <see cref="CancellationToken"/> that propagates notification if the operations should be canceled.
    /// </param>
    /// <returns>
    /// The <see cref="Task"/> proxy that represents the <see cref="HttpResponse"/>.
    /// </returns>
    public new Task<HttpResponse<GetDocumentsResult<TModel>>> RunAndGet(
        IEnumerable<Document>? cacheDocuments = default,
        Transaction? transaction = default,
        IAuthorization? authorization = default,
        CancellationToken cancellationToken = default)
        => RunAndGet<TModel>(cacheDocuments, transaction, authorization, cancellationToken);
}

public partial class FluentWriteWithCacheAndDocumentTransform<TWrite>
{
    /// <summary>
    /// Adds a cache documents.
    /// </summary>
    /// <param name="documents">
    /// The cache documents.
    /// </param>
    /// <returns>
    /// The write with new added cache documents.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="documents"/> is a null reference.
    /// </exception>
    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    public TWrite Cache(params Document[] documents)
    {
        ArgumentNullException.ThrowIfNull(documents);

        WritableCacheDocuments.AddRange(documents);

        return (TWrite)this;
    }

    /// <summary>
    /// Adds a cache documents.
    /// </summary>
    /// <param name="documents">
    /// The cache documents.
    /// </param>
    /// <returns>
    /// The write with new added cache documents.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="documents"/> is a null reference.
    /// </exception>
    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    public TWrite Cache(IEnumerable<Document> documents)
    {
        ArgumentNullException.ThrowIfNull(documents);

        WritableCacheDocuments.AddRange(documents);

        return (TWrite)this;
    }

    /// <summary>
    /// Runs the write operation.
    /// </summary>
    /// <param name="transaction">
    /// The <see cref="Transaction"/> to optionally perform an atomic operation.
    /// </param>
    /// <param name="authorization">
    /// The authorization used for the operation.
    /// </param>
    /// <param name="cancellationToken">
    /// The <see cref="CancellationToken"/> that propagates notification if the operations should be canceled.
    /// </param>
    /// <returns>
    /// The <see cref="Task"/> proxy that represents the <see cref="HttpResponse"/>.
    /// </returns>
    public override Task<HttpResponse<GetDocumentsResult>> RunAndGet(
        Transaction? transaction = default,
        IAuthorization? authorization = default,
        CancellationToken cancellationToken = default)
    {
        return RunAndGet(CacheDocuments, transaction, authorization, cancellationToken);
    }
}

public partial class FluentWriteWithCacheAndDocumentTransform<TWrite, TModel>
{
    /// <summary>
    /// Runs the write operation.
    /// </summary>
    /// <param name="transaction">
    /// The <see cref="Transaction"/> to optionally perform an atomic operation.
    /// </param>
    /// <param name="authorization">
    /// The authorization used for the operation.
    /// </param>
    /// <param name="cancellationToken">
    /// The <see cref="CancellationToken"/> that propagates notification if the operations should be canceled.
    /// </param>
    /// <returns>
    /// The <see cref="Task"/> proxy that represents the <see cref="HttpResponse"/>.
    /// </returns>
    public override Task<HttpResponse<GetDocumentsResult>> RunAndGet(
        Transaction? transaction = default,
        IAuthorization? authorization = default,
        CancellationToken cancellationToken = default)
    {
        return RunAndGet(CacheDocuments, transaction, authorization, cancellationToken);
    }
}
