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
using RestfulFirebase.Common.Internals;
using RestfulFirebase.FirestoreDatabase.Models;

namespace RestfulFirebase.Api;

/// <summary>
/// Provides firebase cloud firestore database implementations.
/// </summary>
public static partial class FirestoreDatabase
{
    /// <inheritdoc cref="GetDocumentRequest{T}.Execute"/>
    /// <param name="request">
    /// The request of the operation.
    /// </param>
#if NET5_0_OR_GREATER
    [RequiresUnreferencedCode(Message.RequiresUnreferencedCodeMessage)]
#endif
    public static Task<TransactionResponse<GetDocumentRequest<T>, Document<T>>> GetDocument<T>(GetDocumentRequest<T> request)
        where T : class => request.Execute();

    /// <inheritdoc cref="GetDocumentsRequest{T}.Execute"/>
    /// <param name="request">
    /// The request of the operation.
    /// </param>
#if NET5_0_OR_GREATER
    [RequiresUnreferencedCode(Message.RequiresUnreferencedCodeMessage)]
#endif
    public static Task<TransactionResponse<GetDocumentsRequest<T>, BatchGetDocuments<T>>> GetDocuments<T>(GetDocumentsRequest<T> request)
        where T : class => request.Execute();

    /// <inheritdoc cref="WriteDocumentRequest{T}.Execute"/>
    /// <param name="request">
    /// The request of the operation.
    /// </param>
#if NET5_0_OR_GREATER
    [RequiresUnreferencedCode(Message.RequiresUnreferencedCodeMessage)]
#endif
    public static Task<TransactionResponse<WriteDocumentRequest<T>, Document<T>>> WriteDocument<T>(WriteDocumentRequest<T> request)
        where T : class => request.Execute();

    /// <inheritdoc cref="WriteDocumentsRequest{T}.Execute"/>
    /// <param name="request">
    /// The request of the operation.
    /// </param>
#if NET5_0_OR_GREATER
    [RequiresUnreferencedCode(Message.RequiresUnreferencedCodeMessage)]
#endif
    public static Task<TransactionResponse<WriteDocumentsRequest<T>>> WriteDocuments<T>(WriteDocumentsRequest<T> request)
        where T : class => request.Execute();

    /// <inheritdoc cref="DeleteDocumentRequest.Execute"/>
    /// <param name="request">
    /// The request of the operation.
    /// </param>
    public static Task<TransactionResponse<DeleteDocumentRequest>> DeleteDocument(DeleteDocumentRequest request)
        => request.Execute();

    /// <inheritdoc cref="DeleteDocumentsRequest.Execute"/>
    /// <param name="request">
    /// The request of the operation.
    /// </param>
    public static Task<TransactionResponse<DeleteDocumentsRequest>> DeleteDocuments(DeleteDocumentsRequest request)
        => request.Execute();
}
