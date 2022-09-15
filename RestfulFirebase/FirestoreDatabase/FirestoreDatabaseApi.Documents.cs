using RestfulFirebase.FirestoreDatabase;
using System;
using System.Threading.Tasks;
using System.Text.Json;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Collections.Generic;
using RestfulFirebase.FirestoreDatabase.Transactions;
using System.Transactions;
using RestfulFirebase.Common.Transactions;
using RestfulFirebase.Common;

namespace RestfulFirebase.Api;

/// <summary>
/// Provides firebase cloud firestore database implementations.
/// </summary>
public static partial class FirestoreDatabase
{
    /// <inheritdoc cref="BatchGetDocumentRequest{T}.Execute"/>
    /// <param name="request">
    /// The request of the operation.
    /// </param>
#if NET5_0_OR_GREATER
    [RequiresUnreferencedCode(Message.RequiresUnreferencedCodeMessage)]
#endif
    public static Task<BatchGetDocumentResponse<T>> BatchGet<T>(BatchGetDocumentRequest<T> request)
        where T : class => request.Execute();

    /// <inheritdoc cref="GetDocumentRequest{T}.Execute"/>
    /// <param name="request">
    /// The request of the operation.
    /// </param>
#if NET5_0_OR_GREATER
    [RequiresUnreferencedCode(Message.RequiresUnreferencedCodeMessage)]
#endif
    public static Task<GetDocumentResponse<T>> GetDocument<T>(GetDocumentRequest<T> request)
        where T : class => request.Execute();

    /// <inheritdoc cref="PatchDocumentRequest{T}.Execute"/>
    /// <param name="request">
    /// The request of the operation.
    /// </param>
#if NET5_0_OR_GREATER
    [RequiresUnreferencedCode(Message.RequiresUnreferencedCodeMessage)]
#endif
    public static Task<PatchDocumentResponse<T>> PatchDocument<T>(PatchDocumentRequest<T> request)
        where T : class => request.Execute();

    /// <inheritdoc cref="DeleteDocumentRequest.Execute"/>
    /// <param name="request">
    /// The request of the operation.
    /// </param>
    public static Task<DeleteDocumentResponse> DeleteDocument(DeleteDocumentRequest request)
        => request.Execute();
}
