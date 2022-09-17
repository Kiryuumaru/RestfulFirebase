using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using RestfulFirebase.FirestoreDatabase.Queries;
using RestfulFirebase.FirestoreDatabase;
using RestfulFirebase.Common.Transactions;
using System.Threading.Tasks;
using System.IO;
using System.Net.Http;
using System.Diagnostics.CodeAnalysis;
using RestfulFirebase.Common;
using RestfulFirebase.FirestoreDatabase.Models;

namespace RestfulFirebase.FirestoreDatabase.Transactions;

/// <summary>
/// Request to get the <see cref="Document{T}"/> of the specified request query.
/// </summary>
/// <typeparam name="T">
/// The type of the model to populate the document fields.
/// </typeparam>
public class GetDocumentRequest<T> : FirestoreDatabaseRequest<TransactionResponse<GetDocumentRequest<T>, Document<T>>>
    where T : class
{
    /// <summary>
    /// Gets or sets the <see cref="JsonSerializerOptions"/> used to serialize and deserialize documents.
    /// </summary>
    public JsonSerializerOptions? JsonSerializerOptions { get; set; }

    /// <summary>
    /// Gets or sets the requested <see cref="Document{T}"/>.
    /// </summary>
    public Document<T>? Document { get; set; }

    /// <inheritdoc cref="GetDocumentRequest{T}"/>
    /// <returns>
    /// The <see cref="Task"/> proxy that represents the <see cref="TransactionResponse"/> with the result <see cref="Document{T}"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <see cref="TransactionRequest.Config"/> or
    /// <see cref="Document"/> is a null reference.
    /// </exception>
    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    internal override async Task<TransactionResponse<GetDocumentRequest<T>, Document<T>>> Execute()
    {
        ArgumentNullException.ThrowIfNull(Config);
        ArgumentNullException.ThrowIfNull(Document);

        JsonSerializerOptions jsonSerializerOptions = ConfigureJsonSerializerOption(JsonSerializerOptions);

        try
        {
            var response = await Execute(HttpMethod.Get, Document.Reference.BuildUrl(Config.ProjectId));
            using Stream contentStream = await response.Content.ReadAsStreamAsync();
            JsonDocument jsonDocument = await JsonDocument.ParseAsync(contentStream);
            var parsedDocument = ParseDocument(Document.Reference, Document.Model, Document, jsonDocument.RootElement.EnumerateObject(), jsonSerializerOptions);

            return new(this, parsedDocument, null);
        }
        catch (Exception ex)
        {
            return new(this, null, ex);
        }
    }
}
