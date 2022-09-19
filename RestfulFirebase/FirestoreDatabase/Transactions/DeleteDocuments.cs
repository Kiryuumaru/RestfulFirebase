using System;
using System.Collections.Generic;
using System.Text;
using RestfulFirebase.FirestoreDatabase.References;
using RestfulFirebase.FirestoreDatabase;
using RestfulFirebase.FirestoreDatabase.Abstractions;
using RestfulFirebase.Common.Transactions;
using System.Threading.Tasks;
using System.Net.Http;
using RestfulFirebase.Common;
using System.Diagnostics.CodeAnalysis;
using RestfulFirebase.FirestoreDatabase.Models;
using System.Xml.Linq;
using System.IO;
using System.Text.Json;

namespace RestfulFirebase.FirestoreDatabase.Transactions;

/// <summary>
/// Request to delete the <see cref="Document{T}"/> of the specified request query.
/// </summary>
public class DeleteDocumentsRequest : FirestoreDatabaseRequest<TransactionResponse<DeleteDocumentsRequest>>
{
    /// <summary>
    /// Gets or sets the requested <see cref="Document"/> documents.
    /// </summary>
    public IEnumerable<Document>? Documents { get; set; }

    /// <summary>
    /// Gets or sets the requested <see cref="DocumentReference"/> documents.
    /// </summary>
    public IEnumerable<DocumentReference>? DocumentReferences { get; set; }

    /// <inheritdoc cref="DeleteDocumentsRequest"/>
    /// <returns>
    /// The <see cref="Task"/> proxy that represents the <see cref="TransactionResponse"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <see cref="TransactionRequest.Config"/> is a null reference.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <see cref="Documents"/> and
    /// <see cref="DocumentReferences"/> is a null reference.
    /// </exception>
    internal override async Task<TransactionResponse<DeleteDocumentsRequest>> Execute()
    {
        ArgumentNullException.ThrowIfNull(Config);
        if (Documents == null && DocumentReferences == null)
        {
            throw new ArgumentException($"Both {nameof(Documents)} and {nameof(DocumentReferences)} is a null reference. Provide at least one argument.");
        }

        try
        {
            using MemoryStream stream = new();
            Utf8JsonWriter writer = new(stream);

            writer.WriteStartObject();
            writer.WritePropertyName("writes");
            writer.WriteStartArray();
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
