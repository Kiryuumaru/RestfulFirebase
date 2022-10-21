﻿using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using System.Threading;
using RestfulFirebase.FirestoreDatabase.Transactions;
using RestfulFirebase.Common.Http;
using RestfulFirebase.Common.Abstractions;
using RestfulFirebase.FirestoreDatabase.Writes;

namespace RestfulFirebase.FirestoreDatabase.Models;

public partial class Document
{
    /// <summary>
    /// Request to perform a get operation to document.
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
    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    public async Task<HttpResponse> Get(Transaction? transaction = default, IAuthorization? authorization = default, CancellationToken cancellationToken = default)
    {
        HttpResponse response = new();

        var writeResponse = await Reference.GetDocument(new Document[] { this }, transaction, authorization, cancellationToken);
        response.Append(writeResponse);
        if (writeResponse.IsError)
        {
            return response;
        }

        return response;
    }

    /// <summary>
    /// Request to perform a patch and get operation to document.
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
    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    public async Task<HttpResponse> PatchAndGet(Transaction? transaction = default, IAuthorization? authorization = default, CancellationToken cancellationToken = default)
    {
        HttpResponse response = new();

        var writeResponse = await Reference.PatchAndGetDocument(GetModel(), new Document[] { this }, transaction, authorization, cancellationToken);
        response.Append(writeResponse);
        if (writeResponse.IsError)
        {
            return response;
        }

        return response;
    }

    /// <summary>
    /// Adds new <see cref="DocumentTransform"/> to perform a transform operation.
    /// </summary>
    /// <returns>
    /// The write with new added <see cref="DocumentTransform"/> to transform.
    /// </returns>
    public WriteWithCacheAndDocumentTransform Transform()
    {
        return new WriteWithCacheAndDocumentTransform(Reference.Transform())
            .Cache(this);
    }

    /// <summary>
    /// Adds new <see cref="DocumentTransform"/> to perform a transform operation.
    /// </summary>
    /// <typeparam name="TModel">
    /// The type of the model of the document to transform.
    /// </typeparam>
    /// <returns>
    /// The write with new added <see cref="DocumentTransform"/> to transform.
    /// </returns>
    public WriteWithCacheAndDocumentTransform<TModel> Transform<TModel>()
        where TModel : class
    {
        return new WriteWithCacheAndDocumentTransform<TModel>(Reference.Transform<TModel>())
            .Cache(this);
    }
}

public partial class Document<TModel>
{
    /// <summary>
    /// Adds new <see cref="DocumentTransform"/> to perform a transform operation.
    /// </summary>
    /// <returns>
    /// The write with new added <see cref="DocumentTransform"/> to transform.
    /// </returns>
    public new WriteWithCacheAndDocumentTransform<TModel> Transform()
    {
        return new WriteWithCacheAndDocumentTransform<TModel>(Reference.Transform<TModel>())
            .Cache(this);
    }
}
