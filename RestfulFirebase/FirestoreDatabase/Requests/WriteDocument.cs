using System;
using System.Text.Json;
using RestfulFirebase.Common.Requests;
using System.Threading.Tasks;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net.Http;
using RestfulFirebase.FirestoreDatabase.Models;
using RestfulFirebase.FirestoreDatabase.Transactions;
using System.Collections.Generic;

namespace RestfulFirebase.FirestoreDatabase.Requests;

/// <summary>
/// Request to patch the <see cref="Document{T}"/> of the specified request query.
/// </summary>
/// <typeparam name="T">
/// The type of the model to populate the document fields.
/// </typeparam>
public class WriteDocumentRequest<T> : FirestoreDatabaseRequest<TransactionResponse<WriteDocumentRequest<T>>>
    where T : class
{
    /// <summary>
    /// Gets or sets the <see cref="JsonSerializerOptions"/> used to serialize and deserialize documents.
    /// </summary>
    public JsonSerializerOptions? JsonSerializerOptions { get; set; }

    /// <summary>
    /// Gets or sets the requested <see cref="Document{T}"/> to populate the document fields.
    /// </summary>
    public Document<T>? Document { get; set; }

    /// <summary>
    /// Gets or sets the requested <see cref="Document{T}"/> to populate the document fields.
    /// </summary>
    public IEnumerable<Document<T>>? Documents { get; set; }

    /// <summary>
    /// Gets or sets the <see cref="Transactions.Transaction"/> for atomic operation.
    /// </summary>
    public Transaction? Transaction { get; set; }

    /// <inheritdoc cref="TransactionResponse{T}"/>
    /// <returns>
    /// The <see cref="Task"/> proxy that represents the <see cref="TransactionResponse"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <see cref="TransactionRequest.Config"/> is a null reference.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <see cref="Document"/> and
    /// <see cref="Documents"/> is a null reference.
    /// </exception>
    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    internal override async Task<TransactionResponse<WriteDocumentRequest<T>>> Execute()
    {
        ArgumentNullException.ThrowIfNull(Config);
        if (Document == null && Documents == null)
        {
            throw new ArgumentException($"Both {nameof(Document)} and {nameof(Documents)} is a null reference. Provide at least one argument.");
        }

        JsonSerializerOptions jsonSerializerOptions = ConfigureJsonSerializerOption(JsonSerializerOptions);

        try
        {
            using MemoryStream stream = new();
            Utf8JsonWriter writer = new(stream);

            writer.WriteStartObject();
            writer.WritePropertyName("writes");
            writer.WriteStartArray();
            if (Document != null)
            {
                if (Document.Model != null)
                {
                    writer.WriteStartObject();
                    writer.WritePropertyName("update");
                    writer.WriteStartObject();
                    writer.WritePropertyName("name");
                    writer.WriteStringValue(Document.Reference.BuildUrlCascade(Config.ProjectId));
                    writer.WritePropertyName("fields");
                    PopulateDocument(Config, writer, Document.Model, null, jsonSerializerOptions);
                    writer.WriteEndObject();
                    writer.WriteEndObject();
                }
                else
                {
                    writer.WriteStartObject();
                    writer.WritePropertyName("delete");
                    writer.WriteStringValue(Document.Reference.BuildUrlCascade(Config.ProjectId));
                    writer.WriteEndObject();
                }
            }
            if (Documents != null)
            {
                foreach (var document in Documents)
                {
                    if (document.Model != null)
                    {
                        writer.WriteStartObject();
                        writer.WritePropertyName("update");
                        writer.WriteStartObject();
                        writer.WritePropertyName("name");
                        writer.WriteStringValue(document.Reference.BuildUrlCascade(Config.ProjectId));
                        writer.WritePropertyName("fields");
                        PopulateDocument(Config, writer, document.Model, null, jsonSerializerOptions);
                        writer.WriteEndObject();
                        writer.WriteEndObject();
                    }
                    else
                    {
                        writer.WriteStartObject();
                        writer.WritePropertyName("delete");
                        writer.WriteStringValue(document.Reference.BuildUrlCascade(Config.ProjectId));
                        writer.WriteEndObject();
                    }
                }
            }
            writer.WriteEndArray();
            if (Transaction != null)
            {
                writer.WritePropertyName("transaction");
                writer.WriteStringValue(Transaction.Token);
            }
            writer.WriteEndObject();

            await writer.FlushAsync();

            await ExecuteWithContent(stream, HttpMethod.Post, BuildUrl());

            return new(this, null);
        }
        catch (Exception ex)
        {
            return new(this, ex);
        }
    }

    internal string BuildUrl()
    {
        ArgumentNullException.ThrowIfNull(Config);

        return
            $"{Api.FirestoreDatabase.FirestoreDatabaseV1Endpoint}/" +
            $"{string.Format(Api.FirestoreDatabase.FirestoreDatabaseDocumentsEndpoint, Config.ProjectId, ":commit")}";
    }
}
