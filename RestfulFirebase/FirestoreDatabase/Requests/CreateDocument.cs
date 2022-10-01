using System.Text.Json;
using RestfulFirebase.Common.Requests;
using System.Threading.Tasks;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net.Http;
using RestfulFirebase.FirestoreDatabase.Models;
using RestfulFirebase.FirestoreDatabase.References;
using RestfulFirebase.Common.Utilities;
using RestfulFirebase.FirestoreDatabase.Utilities;
using System.Collections.Generic;
using System.Threading;
using System;

namespace RestfulFirebase.FirestoreDatabase.Requests;

internal static class CreateDocumentRequestHelpers
{
    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    public static async Task<object?> ExecuteQuery<TRequest, TResult>(
        BaseCreateDocumentRequest<TRequest, TResult> request,
        FirebaseConfig config,
        CollectionReference collectionReference,
        Type? objType,
        object? obj,
        JsonSerializerOptions jsonSerializerOptions,
        CancellationToken cancellationToken)
        where TRequest : TransactionRequest
    {
        CancellationToken linkedCancellationToken = CancellationTokenSource.CreateLinkedTokenSource(request.CancellationToken, cancellationToken).Token;

        QueryBuilder qb = new();
        if (request.DocumentId != null)
        {
            qb.Add("documentId", request.DocumentId);
        }
        string url = collectionReference.BuildUrl(config.ProjectId, qb.Build());

        using MemoryStream stream = new();
        Utf8JsonWriter writer = new(stream);

        writer.WriteStartObject();
        writer.WritePropertyName("fields");
        if (objType != null)
        {
            ModelHelpers.BuildUtf8JsonWriter(config, writer, objType, obj, null, jsonSerializerOptions);
        }
        else
        {
            writer.WriteStartObject();
            writer.WriteEndObject();
        }
        writer.WriteEndObject();

        await writer.FlushAsync(linkedCancellationToken);

        var (executeResult, executeException) = await request.ExecuteWithContent(stream, HttpMethod.Post, url);
        if (executeResult == null)
        {
            return executeException;
        }

#if NET6_0_OR_GREATER
        using Stream contentStream = await executeResult.Content.ReadAsStreamAsync(linkedCancellationToken);
#else
        using Stream contentStream = await executeResult.Content.ReadAsStreamAsync();
#endif
        return await JsonDocument.ParseAsync(contentStream, cancellationToken: linkedCancellationToken);
    }
}

/// <summary>
/// Request to create a document of the specified request query.
/// </summary>
public abstract class BaseCreateDocumentRequest<TRequest, TResult> : FirestoreDatabaseRequest<TransactionResponse<TRequest, TResult>>
    where TRequest : TransactionRequest
{
    /// <summary>
    /// Gets or sets the <see cref="System.Text.Json.JsonSerializerOptions"/> used to serialize and deserialize documents.
    /// </summary>
    public JsonSerializerOptions? JsonSerializerOptions { get; set; }

    /// <summary>
    /// Gets or sets the requested <see cref="References.CollectionReference"/> of the collection node.
    /// </summary>
    public CollectionReference? CollectionReference { get; set; }

    /// <summary>
    /// Gets or sets the client-assigned document ID to use for this document. Optional. If not specified, an ID will be assigned by the service.
    /// </summary>
    public string? DocumentId { get; set; }
}

/// <summary>
/// Request to create a <see cref="Document"/> of the specified request query.
/// </summary>
public class CreateDocumentRequest : BaseCreateDocumentRequest<CreateDocumentRequest, Models.Document>
{
    /// <summary>
    /// Gets or sets the type of the model to create the document.
    /// </summary>
    public Type? ModelType { get; set; }

    /// <summary>
    /// Gets or sets the model to create the document.
    /// </summary>
    public object? Model { get; set; }

    /// <inheritdoc cref="CreateDocumentRequest"/>
    /// <returns>
    /// The <see cref="Task"/> proxy that represents the <see cref="TransactionResponse"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <see cref="TransactionRequest.Config"/>,
    /// <see cref="Model"/> or
    /// <see cref="CollectionReference"/> is a null reference.
    /// </exception>
    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    internal override async Task<TransactionResponse<CreateDocumentRequest, Models.Document>> Execute()
    {
        ArgumentNullException.ThrowIfNull(Config);
        ArgumentNullException.ThrowIfNull(Model);
        ArgumentNullException.ThrowIfNull(CollectionReference);

        JsonSerializerOptions jsonSerializerOptions = ConfigureJsonSerializerOption(JsonSerializerOptions);

        object? result = await CreateDocumentRequestHelpers.ExecuteQuery(
            this,
            Config,
            CollectionReference,
            ModelType,
            Model,
            jsonSerializerOptions,
            default);

        if (result is not JsonDocument jsonDocument)
        {
            return new(this, null, result as Exception);
        }

        var parsedDocument = Models.Document.Parse(null, ModelType ?? Model?.GetType(), Model, null, jsonDocument.RootElement.EnumerateObject(), jsonSerializerOptions);

        return new(this, parsedDocument, null);
    }
}

/// <summary>
/// Request to create a <see cref="Document{T}"/> of the specified request query.
/// </summary>
/// <typeparam name="T">
/// The type of the model to populate the document fields.
/// </typeparam>
public class CreateDocumentRequest<T> : BaseCreateDocumentRequest<CreateDocumentRequest<T>, Document<T>>
    where T : class
{
    /// <summary>
    /// Gets or sets the <typeparamref name="T"/> to create the document.
    /// </summary>
    public T? Model { get; set; }

    /// <inheritdoc cref="CreateDocumentRequest{T}"/>
    /// <returns>
    /// The <see cref="Task"/> proxy that represents the <see cref="TransactionResponse"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <see cref="TransactionRequest.Config"/>,
    /// <see cref="Model"/> or
    /// <see cref="CollectionReference"/> is a null reference.
    /// </exception>
    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    internal override async Task<TransactionResponse<CreateDocumentRequest<T>, Document<T>>> Execute()
    {
        ArgumentNullException.ThrowIfNull(Config);
        ArgumentNullException.ThrowIfNull(Model);
        ArgumentNullException.ThrowIfNull(CollectionReference);

        JsonSerializerOptions jsonSerializerOptions = ConfigureJsonSerializerOption(JsonSerializerOptions);

        object? result = await CreateDocumentRequestHelpers.ExecuteQuery(
            this,
            Config,
            CollectionReference,
            typeof(T),
            Model,
            jsonSerializerOptions,
            default);

        if (result is not JsonDocument jsonDocument)
        {
            return new(this, null, result as Exception);
        }

        var parsedDocument = Document<T>.Parse(null, Model, null, jsonDocument.RootElement.EnumerateObject(), jsonSerializerOptions);

        return new(this, parsedDocument, null);
    }
}
