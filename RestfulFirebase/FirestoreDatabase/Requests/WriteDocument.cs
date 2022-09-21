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
using RestfulFirebase.FirestoreDatabase.References;

namespace RestfulFirebase.FirestoreDatabase.Requests;

/// <summary>
/// Request to patch the <see cref="Document{T}"/> of the specified request query.
/// </summary>
public abstract class BaseWriteDocumentRequest<TDocument> : FirestoreDatabaseRequest<TransactionResponse<BaseWriteDocumentRequest<TDocument>>>
    where TDocument : Document
{
    /// <summary>
    /// Gets or sets the <see cref="JsonSerializerOptions"/> used to serialize and deserialize documents.
    /// </summary>
    public JsonSerializerOptions? JsonSerializerOptions { get; set; }

    /// <summary>
    /// Gets or sets the requested <see cref="Document{T}"/> to patch the document fields. If <see cref="Document{T}.Model"/> is a null reference, operation will delete the document.
    /// </summary>
    public TDocument? PatchDocument { get; set; }

    /// <summary>
    /// Gets or sets the requested <see cref="Document{T}"/> to patch the document fields. If <see cref="Document{T}.Model"/> is a null reference, operation will delete the document.
    /// </summary>
    public IEnumerable<TDocument>? PatchDocuments { get; set; }

    /// <summary>
    /// Gets or sets the requested <see cref="Document{T}"/> to delete the document fields.
    /// </summary>
    public TDocument? DeleteDocument { get; set; }

    /// <summary>
    /// Gets or sets the requested <see cref="Document{T}"/> to delete the document fields.
    /// </summary>
    public IEnumerable<TDocument>? DeleteDocuments { get; set; }

    /// <summary>
    /// Gets or sets the requested <see cref="DocumentReference"/> of the document node to delete.
    /// </summary>
    public DocumentReference? DeleteDocumentReference { get; set; }

    /// <summary>
    /// Gets or sets the requested <see cref="DocumentReference"/> of the document node to delete.
    /// </summary>
    public IEnumerable<DocumentReference>? DeleteDocumentReferences { get; set; }

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
    /// <see cref="PatchDocument"/>,
    /// <see cref="PatchDocuments"/>,
    /// <see cref="DeleteDocument"/>,
    /// <see cref="DeleteDocuments"/>,
    /// <see cref="DeleteDocumentReference"/> and
    /// <see cref="DeleteDocumentReferences"/> are a null reference.
    /// </exception>
    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    internal override async Task<TransactionResponse<BaseWriteDocumentRequest<TDocument>>> Execute()
    {
        ArgumentNullException.ThrowIfNull(Config);
        if (PatchDocument == null &&
            PatchDocuments == null &&
            DeleteDocument == null &&
            DeleteDocuments == null &&
            DeleteDocumentReference == null &&
            DeleteDocumentReferences == null)
        {
            throw new ArgumentException($"All {nameof(PatchDocument)}, {nameof(PatchDocuments)}, {nameof(DeleteDocument)}, {nameof(DeleteDocuments)}, {nameof(DeleteDocumentReference)} and {nameof(DeleteDocumentReferences)} are a null reference. Provide at least one argument.");
        }

        JsonSerializerOptions jsonSerializerOptions = ConfigureJsonSerializerOption(JsonSerializerOptions);

        try
        {
            using MemoryStream stream = new();
            Utf8JsonWriter writer = new(stream);

            writer.WriteStartObject();
            writer.WritePropertyName("writes");
            writer.WriteStartArray();
            if (PatchDocument != null)
            {
                if (PatchDocument.GetModel() is object obj)
                {
                    writer.WriteStartObject();
                    writer.WritePropertyName("update");
                    writer.WriteStartObject();
                    writer.WritePropertyName("name");
                    writer.WriteStringValue(PatchDocument.Reference.BuildUrlCascade(Config.ProjectId));
                    writer.WritePropertyName("fields");
                    PopulateDocument(Config, writer, obj.GetType(), obj, PatchDocument, jsonSerializerOptions);
                    writer.WriteEndObject();
                    writer.WriteEndObject();
                }
                else
                {
                    writer.WriteStartObject();
                    writer.WritePropertyName("delete");
                    writer.WriteStringValue(PatchDocument.Reference.BuildUrlCascade(Config.ProjectId));
                    writer.WriteEndObject();
                }
            }
            if (PatchDocuments != null)
            {
                foreach (var document in PatchDocuments)
                {
                    if (document.GetModel() is object obj)
                    {
                        writer.WriteStartObject();
                        writer.WritePropertyName("update");
                        writer.WriteStartObject();
                        writer.WritePropertyName("name");
                        writer.WriteStringValue(document.Reference.BuildUrlCascade(Config.ProjectId));
                        writer.WritePropertyName("fields");
                        PopulateDocument(Config, writer, obj.GetType(), obj, document, jsonSerializerOptions);
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
            if (DeleteDocument != null)
            {
                writer.WriteStartObject();
                writer.WritePropertyName("delete");
                writer.WriteStringValue(DeleteDocument.Reference.BuildUrlCascade(Config.ProjectId));
                writer.WriteEndObject();
            }
            if (DeleteDocuments != null)
            {
                foreach (var document in DeleteDocuments)
                {
                    writer.WriteStartObject();
                    writer.WritePropertyName("delete");
                    writer.WriteStringValue(document.Reference.BuildUrlCascade(Config.ProjectId));
                    writer.WriteEndObject();
                }
            }
            if (DeleteDocumentReference != null)
            {
                writer.WriteStartObject();
                writer.WritePropertyName("delete");
                writer.WriteStringValue(DeleteDocumentReference.BuildUrlCascade(Config.ProjectId));
                writer.WriteEndObject();
            }
            if (DeleteDocumentReferences != null)
            {
                foreach (var reference in DeleteDocumentReferences)
                {
                    writer.WriteStartObject();
                    writer.WritePropertyName("delete");
                    writer.WriteStringValue(reference.BuildUrlCascade(Config.ProjectId));
                    writer.WriteEndObject();
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

/// <summary>
/// Request to patch the <see cref="Document{T}"/> of the specified request query.
/// </summary>
public class WriteDocumentRequest : BaseWriteDocumentRequest<Document>
{

}

/// <summary>
/// Request to patch the <see cref="Document{T}"/> of the specified request query.
/// </summary>
public class WriteDocumentRequest<T> : BaseWriteDocumentRequest<Document<T>>
    where T : class
{

}
