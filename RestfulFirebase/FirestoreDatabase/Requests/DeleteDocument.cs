using System;
using RestfulFirebase.FirestoreDatabase.References;
using RestfulFirebase.Common.Requests;
using System.Threading.Tasks;
using System.Net.Http;
using RestfulFirebase.FirestoreDatabase.Models;
using System.IO;
using System.Text.Json;
using RestfulFirebase.FirestoreDatabase.Transactions;

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
    /// Gets or sets the requested <see cref="References.DocumentReference"/> of the document node.
    /// </summary>
    public DocumentReference? DocumentReference { get; set; }

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
    /// <see cref="Document"/> and
    /// <see cref="DocumentReference"/> is a null reference.
    /// </exception>
    internal override async Task<TransactionResponse<DeleteDocumentRequest>> Execute()
    {
        ArgumentNullException.ThrowIfNull(Config);
        if (Document == null && DocumentReference == null)
        {
            throw new ArgumentException($"Both {nameof(Document)} and {nameof(DocumentReference)} is a null reference. Provide at least one argument.");
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
            if (DocumentReference != null)
            {
                writer.WriteStartObject();
                writer.WritePropertyName("delete");
                writer.WriteStringValue(DocumentReference.BuildUrlCascade(Config.ProjectId));
                writer.WriteEndObject();
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
