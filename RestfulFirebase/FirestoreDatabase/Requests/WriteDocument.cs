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
using System.Linq;
using RestfulFirebase.FirestoreDatabase.Enums;
using System.Xml.Linq;
using RestfulFirebase.FirestoreDatabase.Transform;
using RestfulFirebase.Common.Utilities;

namespace RestfulFirebase.FirestoreDatabase.Requests;

/// <summary>
/// Request to patch the <see cref="Document{T}"/> of the specified request query.
/// </summary>
public class WriteDocumentRequest : FirestoreDatabaseRequest<TransactionResponse<WriteDocumentRequest>>
{
    /// <summary>
    /// Gets or sets the <see cref="JsonSerializerOptions"/> used to serialize and deserialize documents.
    /// </summary>
    public JsonSerializerOptions? JsonSerializerOptions { get; set; }

    /// <summary>
    /// Gets or sets the requested <see cref="Document{T}"/> to patch the document fields. If <see cref="Document{T}.Model"/> is a null reference, operation will delete the document.
    /// </summary>
    public Document.Builder? PatchDocument { get; set; }

    /// <summary>
    /// Gets or sets the requested <see cref="Document{T}"/> to delete the document.
    /// </summary>
    public Document.Builder? DeleteDocument { get; set; }

    /// <summary>
    /// Gets or sets the requested <see cref="DocumentTransform"/> of the document node to transform.
    /// </summary>
    public DocumentTransform.Builder? TransformDocument { get; set; }

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
    /// <see cref="DeleteDocument"/> and
    /// <see cref="TransformDocument"/> are a null reference.
    /// </exception>
    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    internal override async Task<TransactionResponse<WriteDocumentRequest>> Execute()
    {
        ArgumentNullException.ThrowIfNull(Config);
        if (PatchDocument == null &&
            DeleteDocument == null &&
            TransformDocument == null)
        {
            throw new ArgumentException($"All " +
                $"{nameof(PatchDocument)}, " +
                $"{nameof(DeleteDocument)} and " +
                $"{nameof(TransformDocument)} are a null reference. Provide at least one argument.");
        }

        JsonSerializerOptions jsonSerializerOptions = ConfigureJsonSerializerOption(JsonSerializerOptions);

        using MemoryStream stream = new();
        Utf8JsonWriter writer = new(stream);

        writer.WriteStartObject();
        writer.WritePropertyName("writes");
        writer.WriteStartArray();
        if (PatchDocument != null)
        {
            foreach (var document in PatchDocument.Documents)
            {
                if (document.GetModel() is object obj)
                {
                    writer.WriteStartObject();
                    writer.WritePropertyName("update");
                    writer.WriteStartObject();
                    writer.WritePropertyName("name");
                    writer.WriteStringValue(document.Reference.BuildUrlCascade(Config.ProjectId));
                    writer.WritePropertyName("fields");
                    ModelHelpers.BuildUtf8JsonWriter(Config, writer, obj.GetType(), obj, document, jsonSerializerOptions);
                    writer.WriteEndObject();
                    writer.WriteEndObject();
                }
                else
                {
                    writer.WriteStartObject();
                    writer.WritePropertyName("delete");
                    writer.WriteStringValue(document.Reference.BuildUrlCascade(Config.ProjectId));
                    writer.WriteEndObject();
                    WriteDeleteDocument(writer, document.Reference);
                }
            }
        }
        if (DeleteDocument != null)
        {
            foreach (var document in DeleteDocument.Documents)
            {
                writer.WriteStartObject();
                writer.WritePropertyName("delete");
                writer.WriteStringValue(document.Reference.BuildUrlCascade(Config.ProjectId));
                writer.WriteEndObject();
                WriteDeleteDocument(writer, document.Reference);
            }
        }
        if (TransformDocument != null)
        {
            foreach (var documentTransform in TransformDocument.DocumentTransforms)
            {
                documentTransform.BuildAsUtf8JsonWriter(writer, Config, jsonSerializerOptions);
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

        try
        {
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

    private void WriteDeleteDocument(Utf8JsonWriter writer, DocumentReference documentReference)
    {
        ArgumentNullException.ThrowIfNull(Config);

        writer.WriteStartObject();
        writer.WritePropertyName("delete");
        writer.WriteStringValue(documentReference.BuildUrlCascade(Config.ProjectId));
        writer.WriteEndObject();
    }
}
