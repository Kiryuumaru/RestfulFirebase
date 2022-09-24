using System;
using System.Text.Json;
using RestfulFirebase.Common.Requests;
using System.Threading.Tasks;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net.Http;
using RestfulFirebase.FirestoreDatabase.Models;
using RestfulFirebase.FirestoreDatabase.Transactions;
using RestfulFirebase.FirestoreDatabase.References;
using RestfulFirebase.Common.Utilities;

namespace RestfulFirebase.FirestoreDatabase.Requests;

/// <summary>
/// Request to create the <see cref="Document{T}"/> of the specified request query.
/// </summary>
/// <typeparam name="T">
/// The type of the model to populate the document fields.
/// </typeparam>
public class CreateDocumentRequest<T> : FirestoreDatabaseRequest<TransactionResponse<CreateDocumentRequest<T>, Document<T>>>
    where T : class
{
    /// <summary>
    /// Gets or sets the <see cref="JsonSerializerOptions"/> used to serialize and deserialize documents.
    /// </summary>
    public JsonSerializerOptions? JsonSerializerOptions { get; set; }

    /// <summary>
    /// Gets or sets the <typeparamref name="T"/> to create the document.
    /// </summary>
    public T? Model { get; set; }

    /// <summary>
    /// Gets or sets the requested <see cref="References.CollectionReference"/> of the collection node.
    /// </summary>
    public CollectionReference? CollectionReference { get; set; }

    /// <summary>
    /// Gets or sets the client-assigned document ID to use for this document. Optional. If not specified, an ID will be assigned by the service.
    /// </summary>
    public string? DocumentId { get; set; }

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

        QueryBuilder qb = new();
        if (DocumentId != null)
        {
            qb.Add("documentId", DocumentId);
        }
        string url = CollectionReference.BuildUrl(Config.ProjectId, qb.Build());

        using MemoryStream stream = new();
        Utf8JsonWriter writer = new(stream);

        writer.WriteStartObject();
        writer.WritePropertyName("fields");
        ModelHelpers.BuildUtf8JsonWriter(Config, writer, Model, null, jsonSerializerOptions);
        writer.WriteEndObject();

        await writer.FlushAsync();

        try
        {
            var response = await ExecuteWithContent(stream, HttpMethod.Post, url);
            using Stream contentStream = await response.Content.ReadAsStreamAsync();
            JsonDocument jsonDocument = await JsonDocument.ParseAsync(contentStream);
            var parsedDocument = Document<T>.Parse(null, Model, null, jsonDocument.RootElement.EnumerateObject(), jsonSerializerOptions);

            return new(this, parsedDocument, null);
        }
        catch (Exception ex)
        {
            return new(this, null, ex);
        }
    }
}
