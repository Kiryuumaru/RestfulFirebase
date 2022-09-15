using System;
using System.Collections.Generic;
using System.Text;
using RestfulFirebase.FirestoreDatabase.Query;
using RestfulFirebase.FirestoreDatabase;
using RestfulFirebase.FirestoreDatabase.Abstraction;
using RestfulFirebase.Common.Transactions;
using System.Threading.Tasks;
using System.Net.Http;
using RestfulFirebase.Common;
using System.Diagnostics.CodeAnalysis;

namespace RestfulFirebase.FirestoreDatabase.Transactions;

/// <summary>
/// Request to delete the <see cref="Document{T}"/> of the specified request query.
/// </summary>
public class DeleteDocumentRequest : FirestoreDatabaseRequest<DeleteDocumentResponse>
{
    /// <summary>
    /// Gets or sets the requested <see cref="IDocumentReference"/> of the document node.
    /// </summary>
    public IDocumentReference? Reference
    {
        get => Query as IDocumentReference;
        set => Query = value;
    }

    /// <inheritdoc cref="DeleteDocumentRequest"/>
    /// <returns>
    /// The <see cref="Task"/> proxy that represents the <see cref="DeleteDocumentResponse"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <see cref="TransactionRequest.Config"/> or
    /// <see cref="Reference"/> is a null reference.
    /// </exception>
    internal override async Task<DeleteDocumentResponse> Execute()
    {
        ArgumentNullException.ThrowIfNull(Config);
        ArgumentNullException.ThrowIfNull(Reference);

        try
        {
            await Execute(HttpMethod.Delete, BuildUrl());

            return new DeleteDocumentResponse(this, null);
        }
        catch (Exception ex)
        {
            return new DeleteDocumentResponse(this, ex);
        }
    }
}

/// <summary>
/// The response of the <see cref="DeleteDocumentRequest"/> request.
/// </summary>
public class DeleteDocumentResponse : TransactionResponse<DeleteDocumentRequest>
{
    internal DeleteDocumentResponse(DeleteDocumentRequest request, Exception? error)
        : base(request, error)
    {

    }
}
