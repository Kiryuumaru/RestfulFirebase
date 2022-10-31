using RestfulFirebase.FirestoreDatabase.References;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using System.Threading;
using RestfulFirebase.Common.Http;
using RestfulFirebase.FirestoreDatabase.Transactions;
using RestfulFirebase.Common.Abstractions;
using RestfulFirebase.FirestoreDatabase.Models;
using System.Linq;

namespace RestfulFirebase.FirestoreDatabase.Writes;

public abstract partial class FluentWriteRoot<TWrite>
{
    /// <summary>
    /// Adds a cache documents.
    /// </summary>
    /// <param name="documents">
    /// The cache documents.
    /// </param>
    /// <returns>
    /// The request with new added cache documents.
    /// </returns>
    public TWrite Cache(params Document[] documents)
    {
        TWrite write = (TWrite)Clone();

        if (documents != null)
        {
            write.WritableCacheDocuments.AddRange(documents);
        }

        return write;
    }

    /// <summary>
    /// Adds a cache documents.
    /// </summary>
    /// <param name="documents">
    /// The cache documents.
    /// </param>
    /// <returns>
    /// The request with new added cache documents.
    /// </returns>
    public TWrite Cache(IEnumerable<Document>? documents)
    {
        TWrite write = (TWrite)Clone();

        if (documents != null)
        {
            write.WritableCacheDocuments.AddRange(documents);
        }

        return write;
    }

    /// <summary>
    /// Sets the <see cref="Transactions.Transaction"/> to optionally perform an atomic operation.
    /// </summary>
    /// <returns>
    /// The write with new added transaction.
    /// </returns>
    public TWrite Transaction(Transaction? transaction)
    {
        TWrite write = (TWrite)Clone();

        write.TransactionUsed = transaction;

        return write;
    }

    /// <summary>
    /// Sets the <see cref="Write.AuthorizationUsed"/> by the write.
    /// </summary>
    /// <returns>
    /// The write with new added authorization.
    /// </returns>
    public TWrite Authorization(IAuthorization? authorization)
    {
        TWrite write = (TWrite)Clone();

        write.AuthorizationUsed = authorization;

        return write;
    }

    /// <summary>
    /// Runs the write operation.
    /// </summary>
    /// <param name="cancellationToken">
    /// The <see cref="CancellationToken"/> that propagates notification if the operations should be canceled.
    /// </param>
    /// <returns>
    /// The <see cref="Task"/> proxy that represents the <see cref="HttpResponse"/>.
    /// </returns>
    public async Task<HttpResponse> Run(CancellationToken cancellationToken = default)
    {
        HttpResponse response = new();

        if (PatchDocuments.Count != 0 ||
            DeleteDocuments.Count != 0 ||
            TransformDocuments.Count != 0)
        {
            var commitOperation = await ExecuteCommit(this, cancellationToken);
            response.Append(commitOperation);
            if (commitOperation.IsError)
            {
                return response;
            }
        }
        if (CreateDocuments.Count != 0)
        {
            var createOperation = await ExecuteCreate(this, cancellationToken);
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
    /// <param name="cancellationToken">
    /// The <see cref="CancellationToken"/> that propagates notification if the operations should be canceled.
    /// </param>
    /// <returns>
    /// The <see cref="Task"/> proxy that represents the <see cref="HttpResponse"/>.
    /// </returns>
    public async Task<HttpResponse<GetDocumentsResult>> RunAndGet(CancellationToken cancellationToken = default)
    {
        HttpResponse response = new();
        List<DocumentTimestamp> found = new();
        List<DocumentReferenceTimestamp> missing = new();

        if (PatchDocuments.Count != 0 ||
            DeleteDocuments.Count != 0 ||
            TransformDocuments.Count != 0)
        {
            var commitOperation = await ExecuteCommit(this, cancellationToken);
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

            var getDocumentResponse = await App.FirestoreDatabase.Fetch()
                .DocumentReference(docRefs)
                .Document(docs)
                .Cache(CacheDocuments)
                .Run(cancellationToken);
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
            var createOperation = await ExecuteCreate(this, cancellationToken);
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
    /// <param name="cancellationToken">
    /// The <see cref="CancellationToken"/> that propagates notification if the operations should be canceled.
    /// </param>
    /// <returns>
    /// The <see cref="Task"/> proxy that represents the <see cref="HttpResponse"/>.
    /// </returns>
    public async Task<HttpResponse<GetDocumentsResult<TModel>>> RunAndGet<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TModel>(CancellationToken cancellationToken = default)
        where TModel : class
    {
        HttpResponse response = new();
        List<DocumentTimestamp<TModel>> found = new();
        List<DocumentReferenceTimestamp> missing = new();

        if (PatchDocuments.Count != 0 ||
            DeleteDocuments.Count != 0 ||
            TransformDocuments.Count != 0)
        {
            var commitOperation = await ExecuteCommit(this, cancellationToken);
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

            var getDocumentResponse = await App.FirestoreDatabase.Fetch<TModel>()
                .DocumentReference(docRefs)
                .Document(docs)
                .Document(typedDocs)
                .Cache(CacheDocuments)
                .Run(cancellationToken);
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
            var createOperation = await ExecuteCreate<TModel>(this, cancellationToken);
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
    /// <param name="cancellationToken">
    /// The <see cref="CancellationToken"/> that propagates notification if the operations should be canceled.
    /// </param>
    /// <returns>
    /// The <see cref="Task"/> proxy that represents the <see cref="HttpResponse"/>.
    /// </returns>
    /// <exception cref="System.ArgumentException">
    /// Write operation has multiple or empty document to execute.
    /// </exception>
    public async Task<HttpResponse<GetDocumentResult>> RunAndGetSingle(CancellationToken cancellationToken = default)
    {
        if (PatchDocuments.Count +
            DeleteDocuments.Count +
            TransformDocuments.Count +
            CreateDocuments.Count != 1)
        {
            ArgumentException.Throw($"Write operation has multiple or empty document to execute.");
        }

        HttpResponse<GetDocumentResult> response = new();

        var runResponse = await RunAndGet(cancellationToken);
        response.Append(runResponse);
        if (runResponse.IsError)
        {
            return response;
        }

        response.Append(new GetDocumentResult(runResponse.Result?.Found.FirstOrDefault(), runResponse.Result?.Missing?.FirstOrDefault()));

        return response;
    }

    /// <summary>
    /// Runs the write operation.
    /// </summary>
    /// <param name="cancellationToken">
    /// The <see cref="CancellationToken"/> that propagates notification if the operations should be canceled.
    /// </param>
    /// <returns>
    /// The <see cref="Task"/> proxy that represents the <see cref="HttpResponse"/>.
    /// </returns>
    /// <exception cref="System.ArgumentException">
    /// Write operation has multiple or empty document to execute.
    /// </exception>
    public async Task<HttpResponse<GetDocumentResult<TModel>>> RunAndGetSingle<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TModel>(CancellationToken cancellationToken = default)
        where TModel : class
    {
        if (PatchDocuments.Count +
            DeleteDocuments.Count +
            TransformDocuments.Count +
            CreateDocuments.Count != 1)
        {
            ArgumentException.Throw($"Write operation has multiple or empty document to execute.");
        }

        HttpResponse<GetDocumentResult<TModel>> response = new();

        var runResponse = await RunAndGet<TModel>(cancellationToken);
        response.Append(runResponse);
        if (runResponse.IsError)
        {
            return response;
        }

        response.Append(new GetDocumentResult<TModel>(runResponse.Result?.Found.FirstOrDefault(), runResponse.Result?.Missing?.FirstOrDefault()));

        return response;
    }
}

public partial class FluentWriteWithDocumentTransform<TWrite, TModel>
{
    /// <summary>
    /// Runs the write operation.
    /// </summary>
    /// <param name="cancellationToken">
    /// The <see cref="CancellationToken"/> that propagates notification if the operations should be canceled.
    /// </param>
    /// <returns>
    /// The <see cref="Task"/> proxy that represents the <see cref="HttpResponse"/>.
    /// </returns>
    public new Task<HttpResponse<GetDocumentsResult<TModel>>> RunAndGet(CancellationToken cancellationToken = default)
        => RunAndGet<TModel>(cancellationToken);

    /// <summary>
    /// Runs the write operation.
    /// </summary>
    /// <param name="cancellationToken">
    /// The <see cref="CancellationToken"/> that propagates notification if the operations should be canceled.
    /// </param>
    /// <returns>
    /// The <see cref="Task"/> proxy that represents the <see cref="HttpResponse"/>.
    /// </returns>
    /// <exception cref="System.ArgumentException">
    /// Write operation has multiple or empty document to execute.
    /// </exception>
    public new Task<HttpResponse<GetDocumentResult<TModel>>> RunAndGetSingle(CancellationToken cancellationToken = default)
        => RunAndGetSingle<TModel>(cancellationToken);
}
