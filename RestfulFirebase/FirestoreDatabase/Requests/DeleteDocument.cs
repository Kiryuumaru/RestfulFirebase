using System;
using RestfulFirebase.FirestoreDatabase.References;
using RestfulFirebase.Common.Requests;
using System.Threading.Tasks;
using System.Net.Http;
using RestfulFirebase.FirestoreDatabase.Models;
using System.IO;
using System.Text.Json;
using RestfulFirebase.FirestoreDatabase.Transactions;
using System.Collections.Generic;

namespace RestfulFirebase.FirestoreDatabase.Requests;

/// <summary>
/// Request to delete the <see cref="Document{T}"/> of the specified request query.
/// </summary>
public class DeleteDocumentRequest : FirestoreDatabaseRequest<TransactionResponse<DeleteDocumentRequest>>
{
    /// <summary>
    /// Gets or sets the requested <see cref="Models.Document"/> of the document node.
    /// </summary>
    public Document? Document { get; set; }

    /// <summary>
    /// Gets or sets the requested <see cref="Models.Document"/> of the document node.
    /// </summary>
    public IEnumerable<Document>? Documents { get; set; }

    /// <summary>
    /// Gets or sets the requested <see cref="References.DocumentReference"/> of the document node.
    /// </summary>
    public DocumentReference? DocumentReference { get; set; }

    /// <summary>
    /// Gets or sets the requested <see cref="References.DocumentReference"/> of the document node.
    /// </summary>
    public IEnumerable<DocumentReference>? DocumentReferences { get; set; }

    /// <summary>
    /// Gets or sets the <see cref="Transactions.Transaction"/> for atomic operation.
    /// </summary>
    public Transaction? Transaction { get; set; }

    /// <inheritdoc cref="DeleteDocumentRequest"/>
    /// <returns>
    /// The <see cref="Task"/> proxy that represents the <see cref="TransactionResponse"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <see cref="TransactionRequest.Config"/> is a null reference.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <see cref="Document"/>
    /// <see cref="Documents"/>
    /// <see cref="DocumentReference"/> and
    /// <see cref="DocumentReferences"/> is a null reference.
    /// </exception>
    internal override async Task<TransactionResponse<DeleteDocumentRequest>> Execute()
    {
        ArgumentNullException.ThrowIfNull(Config);
        if (Document == null && Documents == null && DocumentReference == null && DocumentReferences == null)
        {
            throw new ArgumentException($"{nameof(Document)}, {nameof(Documents)}, {nameof(DocumentReference)} and {nameof(DocumentReferences)} are null references. Provide at least one argument.");
        }

        try
        {
            using MemoryStream stream = new();
            Utf8JsonWriter writer = new(stream);

            writer.WriteStartObject();
            writer.WritePropertyName("writes");
            writer.WriteStartArray();
            if (Document != null)
            {
                writer.WriteStartObject();
                writer.WritePropertyName("delete");
                writer.WriteStringValue(Document.Reference.BuildUrlCascade(Config.ProjectId));
                writer.WriteEndObject();
            }
            if (Documents != null)
            {
                foreach (var document in Documents)
                {
                    writer.WriteStartObject();
                    writer.WritePropertyName("delete");
                    writer.WriteStringValue(document.Reference.BuildUrlCascade(Config.ProjectId));
                    writer.WriteEndObject();
                }
            }
            if (DocumentReference != null)
            {
                writer.WriteStartObject();
                writer.WritePropertyName("delete");
                writer.WriteStringValue(DocumentReference.BuildUrlCascade(Config.ProjectId));
                writer.WriteEndObject();
            }
            if (DocumentReferences != null)
            {
                foreach (var reference in DocumentReferences)
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
