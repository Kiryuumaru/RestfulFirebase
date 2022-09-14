using RestfulFirebase.CloudFirestore.Requests;
using RestfulFirebase.FirestoreDatabase;
using System;
using System.Threading.Tasks;
using System.Text.Json;
using System.Diagnostics.CodeAnalysis;
using RestfulFirebase.Common.Requests;
using System.IO;

namespace RestfulFirebase.Api;

/// <summary>
/// Provides firebase cloud firestore database implementations.
/// </summary>
public static partial class FirestoreDatabase
{
    /// <summary>
    /// Creates an instance of <see cref="Database"/> with the specified <paramref name="databaseId"/>
    /// </summary>
    /// <param name="databaseId">
    /// The ID of the database to use. Set to <c>null</c> if the instance will use the default database.
    /// </param>
    /// <returns>
    /// The created <see cref="RestfulFirebase.FirestoreDatabase.Database"/>.
    /// </returns>
    public static Database Database(string? databaseId = default)
    {
        return RestfulFirebase.FirestoreDatabase.Database.Get(databaseId);
    }

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
    /// The created <see cref="Document{T}"/>
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <see cref="CommonRequest.Config"/> and
    /// <see cref="GetDocumentRequest{T}.Reference"/> are either a null reference.
    /// </exception>
    /// <exception cref="OperationCanceledException">
    /// The operation was cancelled.
    /// </exception>
#if NET5_0_OR_GREATER
    [RequiresUnreferencedCode(RequiresUnreferencedCodeMessage)]
#endif
    public static async Task<Document<T>?> GetDocument<T>(GetDocumentRequest<T> request)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(request.Config);
        ArgumentNullException.ThrowIfNull(request.Reference);

        JsonSerializerOptions jsonSerializerOptions = ConfigureJsonSerializerOption(request.JsonSerializerOptions);

        using Stream contentStream = await ExecuteWithGet(request);
        JsonDocument jsonDocument = await JsonDocument.ParseAsync(contentStream);

        return ParseDocument(request.Reference, request.Model, request.Document, jsonDocument.RootElement.EnumerateObject(), jsonSerializerOptions);
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
    /// The created <see cref="Document{T}"/>
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <see cref="CommonRequest.Config"/>,
    /// <see cref="GetDocumentRequest{T}.Reference"/> and
    /// (<see cref="GetDocumentRequest{T}.Document"/> or <see cref="GetDocumentRequest{T}.Model"/>) are either a null reference.
    /// </exception>
    /// <exception cref="OperationCanceledException">
    /// The operation was cancelled.
    /// </exception>
#if NET5_0_OR_GREATER
    [RequiresUnreferencedCode(RequiresUnreferencedCodeMessage)]
#endif
    public static async Task<Document<T>?> PatchDocument<T>(PatchDocumentRequest<T> request)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(request.Config);
        ArgumentNullException.ThrowIfNull(request.Reference);
        if (request.Document == null && request.Model == null)
        {
            throw new ArgumentException($"Both {nameof(request.Document)} and {nameof(request.Model)} is a null reference. Provide at least one to patch.");
        }

        JsonSerializerOptions jsonSerializerOptions = ConfigureJsonSerializerOption(request.JsonSerializerOptions);

        using Stream stream = await PopulateDocument(request.Config, request.Model, request.Document, jsonSerializerOptions);
        using Stream contentStream = await ExecuteWithPatchContent(request, stream);
        JsonDocument jsonDocument = await JsonDocument.ParseAsync(contentStream);

        return ParseDocument(request.Reference, request.Model, request.Document, jsonDocument.RootElement.EnumerateObject(), jsonSerializerOptions);
    }

    /// <summary>
    /// Delete the <see cref="Document{T}"/> of the specified request query.
    /// </summary>
    /// <param name="request">
    /// The request of the operation.
    /// </param>
    /// <returns>
    /// The <see cref="Task"/> proxy that represents the operation.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <see cref="CommonRequest.Config"/> and
    /// <see cref="DeleteDocumentRequest.Reference"/> are either a null reference.
    /// </exception>
    /// <exception cref="OperationCanceledException">
    /// The operation was cancelled.
    /// </exception>
    public static async Task DeleteDocument(DeleteDocumentRequest request)
    {
        ArgumentNullException.ThrowIfNull(request.Config);
        ArgumentNullException.ThrowIfNull(request.Reference);

        using Stream response = await ExecuteWithDelete(request);
    }
}
