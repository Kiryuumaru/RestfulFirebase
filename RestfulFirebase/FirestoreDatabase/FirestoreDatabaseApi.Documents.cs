using System.Threading.Tasks;
using System.Diagnostics.CodeAnalysis;
using RestfulFirebase.FirestoreDatabase.Requests;
using RestfulFirebase.Common.Requests;
using RestfulFirebase.Common.Internals;
using RestfulFirebase.FirestoreDatabase.Models;

namespace RestfulFirebase.Api;

/// <summary>
/// Provides firebase cloud firestore database implementations.
/// </summary>
public static partial class FirestoreDatabase
{
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

    /// <inheritdoc cref="ListCollectionsRequest.Execute"/>
    /// <param name="request">
    /// The request of the operation.
    /// </param>
#if NET5_0_OR_GREATER
    [RequiresUnreferencedCode(Message.RequiresUnreferencedCodeMessage)]
#endif
    public static Task<TransactionResponse<ListCollectionsRequest, ListCollectionsResult>> ListCollections(ListCollectionsRequest request)
        => request.Execute();

    /// <inheritdoc cref="ListDocumentsRequest{T}.Execute"/>
    /// <param name="request">
    /// The request of the operation.
    /// </param>
#if NET5_0_OR_GREATER
    [RequiresUnreferencedCode(Message.RequiresUnreferencedCodeMessage)]
#endif
    public static Task<TransactionResponse<ListDocumentsRequest<T>, ListDocumentsResult<T>>> ListDocuments<T>(ListDocumentsRequest<T> request)
        where T : class => request.Execute();

    /// <inheritdoc cref="WriteDocumentRequest{T}.Execute"/>
    /// <param name="request">
    /// The request of the operation.
    /// </param>
#if NET5_0_OR_GREATER
    [RequiresUnreferencedCode(Message.RequiresUnreferencedCodeMessage)]
#endif
    public static Task<TransactionResponse<WriteDocumentRequest<T>>> WriteDocument<T>(WriteDocumentRequest<T> request)
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
}
