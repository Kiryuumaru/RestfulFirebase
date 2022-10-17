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
    public Task<HttpResponse> Run(
        Transaction? transaction = default,
        IAuthorization? authorization = default,
        CancellationToken cancellationToken = default)
    {
        return App.FirestoreDatabase.ExecuteWrite(this, transaction, authorization, cancellationToken);
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
        IEnumerable<Document>? cacheDocuments = default,
        Transaction? transaction = default,
        IAuthorization? authorization = default,
        CancellationToken cancellationToken = default)
    {
        HttpResponse<GetDocumentsResult> response = new();

        var patchDocumentResponse = await App.FirestoreDatabase.ExecuteWrite(this, transaction, authorization, cancellationToken);
        response.Append(patchDocumentResponse);
        if (patchDocumentResponse.IsError)
        {
            return response;
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
            return response;
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
    public async Task<HttpResponse<GetDocumentsResult<TModel>>> RunAndGet<TModel>(
        IEnumerable<Document>? cacheDocuments = default,
        Transaction? transaction = default,
        IAuthorization? authorization = default,
        CancellationToken cancellationToken = default)
        where TModel : class
    {
        HttpResponse<GetDocumentsResult<TModel>> response = new();

        var patchDocumentResponse = await App.FirestoreDatabase.ExecuteWrite(this, transaction, authorization, cancellationToken);

        response.Append(patchDocumentResponse);
        if (patchDocumentResponse.IsError)
        {
            return response;
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
            return response;
        }

        return response;
    }
}

public partial class FluentWriteRoot<TWrite>
{

}
