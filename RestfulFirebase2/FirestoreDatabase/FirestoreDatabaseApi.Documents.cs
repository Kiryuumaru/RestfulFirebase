﻿using System.Threading.Tasks;
using System.Diagnostics.CodeAnalysis;
using RestfulFirebase.FirestoreDatabase.Requests;
using RestfulFirebase.Common.Requests;
using RestfulFirebase.Common.Internals;
using RestfulFirebase.FirestoreDatabase.Models;
using RestfulFirebase.FirestoreDatabase.Transactions;

namespace RestfulFirebase.Api;

/// <summary>
/// Provides firebase cloud firestore database implementations.
/// </summary>
public static partial class FirestoreDatabase
{
    /// <inheritdoc cref="BeginTransactionRequest.Execute"/>
    /// <param name="request">
    /// The request of the operation.
    /// </param>
    public static Task<TransactionResponse<BeginTransactionRequest, Transaction>> BeginTransaction(BeginTransactionRequest request)
        => request.Execute();

    /// <inheritdoc cref="CreateDocumentRequest.Execute"/>
    /// <param name="request">
    /// The request of the operation.
    /// </param>
#if NET5_0_OR_GREATER
    [RequiresUnreferencedCode(Message.RequiresUnreferencedCodeMessage)]
#endif
    public static Task<TransactionResponse<CreateDocumentRequest, Document>> CreateDocument(CreateDocumentRequest request)
        => request.Execute();

    /// <inheritdoc cref="CreateDocumentRequest{T}.Execute"/>
    /// <param name="request">
    /// The request of the operation.
    /// </param>
#if NET5_0_OR_GREATER
    [RequiresUnreferencedCode(Message.RequiresUnreferencedCodeMessage)]
#endif
    public static Task<TransactionResponse<CreateDocumentRequest<T>, Document<T>>> CreateDocument<T>(CreateDocumentRequest<T> request)
        where T : class => request.Execute();

    /// <inheritdoc cref="GetDocumentRequest.Execute"/>
    /// <param name="request">
    /// The request of the operation.
    /// </param>
#if NET5_0_OR_GREATER
    [RequiresUnreferencedCode(Message.RequiresUnreferencedCodeMessage)]
#endif
    public static Task<TransactionResponse<GetDocumentRequest, GetDocumentResult>> GetDocument(GetDocumentRequest request)
        => request.Execute();

    /// <inheritdoc cref="GetDocumentRequest{T}.Execute"/>
    /// <param name="request">
    /// The request of the operation.
    /// </param>
#if NET5_0_OR_GREATER
    [RequiresUnreferencedCode(Message.RequiresUnreferencedCodeMessage)]
#endif
    public static Task<TransactionResponse<GetDocumentRequest<T>, GetDocumentResult<T>>> GetDocument<T>(GetDocumentRequest<T> request)
        where T : class => request.Execute();

    /// <inheritdoc cref="ListCollectionsRequest.Execute"/>
    /// <param name="request">
    /// The request of the operation.
    /// </param>
#if NET5_0_OR_GREATER
    [RequiresUnreferencedCode(Message.RequiresUnreferencedCodeMessage)]
#endif
    public static Task<TransactionResponse<ListCollectionsRequest, ListCollectionsResult>> ListCollections(ListCollectionsRequest request)
        => request.Execute();

    /// <inheritdoc cref="QueryDocumentRequest.Execute"/>
    /// <param name="request">
    /// The request of the operation.
    /// </param>
#if NET5_0_OR_GREATER
    [RequiresUnreferencedCode(Message.RequiresUnreferencedCodeMessage)]
#endif
    public static Task<TransactionResponse<QueryDocumentRequest, QueryDocumentResult>> QueryDocument(QueryDocumentRequest request)
        => request.Execute();

    /// <inheritdoc cref="QueryDocumentRequest{T}.Execute"/>
    /// <param name="request">
    /// The request of the operation.
    /// </param>
#if NET5_0_OR_GREATER
    [RequiresUnreferencedCode(Message.RequiresUnreferencedCodeMessage)]
#endif
    public static Task<TransactionResponse<QueryDocumentRequest<T>, QueryDocumentResult<T>>> QueryDocument<T>(QueryDocumentRequest<T> request)
        where T : class => request.Execute();

    /// <inheritdoc cref="WriteDocumentRequest.Execute"/>
    /// <param name="request">
    /// The request of the operation.
    /// </param>
#if NET5_0_OR_GREATER
    [RequiresUnreferencedCode(Message.RequiresUnreferencedCodeMessage)]
#endif
    public static Task<TransactionResponse<WriteDocumentRequest>> WriteDocument(WriteDocumentRequest request)
        => request.Execute();
}
