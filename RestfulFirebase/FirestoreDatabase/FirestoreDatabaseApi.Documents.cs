using RestfulFirebase.CloudFirestore.Requests;
using RestfulFirebase.FirestoreDatabase;
using System;
using System.Threading.Tasks;
using System.Text.Json;
using System.Diagnostics.CodeAnalysis;
using RestfulFirebase.Common.Requests;
using System.IO;
using RestfulFirebase.Common.Responses;

namespace RestfulFirebase.Api;

/// <summary>
/// Provides firebase cloud firestore database implementations.
/// </summary>
public static partial class FirestoreDatabase
{
    /// <summary>
    /// Gets the <see cref="Document{T}"/> of the specified request query.
    /// </summary>
    /// <typeparam name="T">
    /// The type of the model to populate the document fields.
    /// </typeparam>
    /// <param name="request">
    /// The request of the operation.
    /// </param>
    /// <returns>
    /// The <see cref="Task"/> proxy that represents the <see cref="CommonResponse"/> with the created <see cref="Document{T}"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <see cref="CommonRequest.Config"/> and
    /// <see cref="GetDocumentRequest{T}.Reference"/> are either a null reference.
    /// </exception>
#if NET5_0_OR_GREATER
    [RequiresUnreferencedCode(RequiresUnreferencedCodeMessage)]
#endif
    public static async Task<CommonResponse<GetDocumentRequest<T>, Document<T>>> GetDocument<T>(GetDocumentRequest<T> request)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(request.Config);
        ArgumentNullException.ThrowIfNull(request.Reference);

        JsonSerializerOptions jsonSerializerOptions = ConfigureJsonSerializerOption(request.JsonSerializerOptions);

        try
        {
            using Stream contentStream = await ExecuteWithGet(request);
            JsonDocument jsonDocument = await JsonDocument.ParseAsync(contentStream);

            return CommonResponse.Create(request, ParseDocument(request.Reference, request.Model, request.Document, jsonDocument.RootElement.EnumerateObject(), jsonSerializerOptions));
        }
        catch (Exception ex)
        {
            return CommonResponse.Create<GetDocumentRequest<T>, Document<T>>(request, null, ex);
        }
    }

    /// <summary>
    /// Patch the <see cref="Document{T}"/> of the specified request query.
    /// </summary>
    /// <typeparam name="T">
    /// The type of the model to populate the document fields.
    /// </typeparam>
    /// <param name="request">
    /// The request of the operation.
    /// </param>
    /// <returns>
    /// The <see cref="Task"/> proxy that represents the <see cref="CommonResponse"/> with the created <see cref="Document{T}"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <see cref="CommonRequest.Config"/>,
    /// <see cref="GetDocumentRequest{T}.Reference"/> and
    /// (<see cref="GetDocumentRequest{T}.Document"/> or <see cref="GetDocumentRequest{T}.Model"/>) are either a null reference.
    /// </exception>
#if NET5_0_OR_GREATER
    [RequiresUnreferencedCode(RequiresUnreferencedCodeMessage)]
#endif
    public static async Task<CommonResponse<PatchDocumentRequest<T>, Document<T>>> PatchDocument<T>(PatchDocumentRequest<T> request)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(request.Config);
        ArgumentNullException.ThrowIfNull(request.Reference);

        if (request.Document == null && request.Model == null)
        {
            throw new ArgumentException($"Both {nameof(request.Document)} and {nameof(request.Model)} is a null reference. Provide at least one to patch.");
        }

        try
        {
            JsonSerializerOptions jsonSerializerOptions = ConfigureJsonSerializerOption(request.JsonSerializerOptions);

            using Stream stream = await PopulateDocument(request.Config, request.Model, request.Document, jsonSerializerOptions);
            using Stream contentStream = await ExecuteWithPatchContent(request, stream);
            JsonDocument jsonDocument = await JsonDocument.ParseAsync(contentStream);

            return CommonResponse.Create(request, ParseDocument(request.Reference, request.Model, request.Document, jsonDocument.RootElement.EnumerateObject(), jsonSerializerOptions));
        }
        catch (Exception ex)
        {
            return CommonResponse.Create<PatchDocumentRequest<T>, Document<T>>(request, null, ex);
        }
    }

    /// <summary>
    /// Delete the <see cref="Document{T}"/> of the specified request query.
    /// </summary>
    /// <param name="request">
    /// The request of the operation.
    /// </param>
    /// <returns>
    /// The <see cref="Task"/> proxy that represents the <see cref="CommonResponse"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <see cref="CommonRequest.Config"/> and
    /// <see cref="DeleteDocumentRequest.Reference"/> are either a null reference.
    /// </exception>
    public static async Task<CommonResponse<DeleteDocumentRequest>> DeleteDocument(DeleteDocumentRequest request)
    {
        ArgumentNullException.ThrowIfNull(request.Config);
        ArgumentNullException.ThrowIfNull(request.Reference);

        try
        {
            using Stream response = await ExecuteWithDelete(request);

            return CommonResponse.Create(request);
        }
        catch (Exception ex)
        {
            return CommonResponse.Create(request, ex);
        }
    }
}
