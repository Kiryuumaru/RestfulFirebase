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

    /// <summary>
    /// Gets or sets the requested <see cref="Queries.DocumentReference"/> of the document node.
    /// </summary>
    public DocumentReference? DocumentReference { get; set; }

    /// <inheritdoc cref="GetDocumentRequest{T}"/>
    /// <returns>
    /// The <see cref="Task"/> proxy that represents the <see cref="TransactionResponse"/> with the result <see cref="Document{T}"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <see cref="TransactionRequest.Config"/> is a null reference.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <see cref="Document"/> and
    /// <see cref="DocumentReference"/> is a null reference.
    /// </exception>
    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    internal override async Task<TransactionResponse<GetDocumentRequest<T>, Document<T>>> Execute()
    {
        ArgumentNullException.ThrowIfNull(Config);

        DocumentReference? documentReference;
        Document<T>? document;
        T? model;
        if (Document != null && DocumentReference != null)
        {
            documentReference = DocumentReference;
            document = Document;
            model = Document.Model;
        }
        else if (Document != null && DocumentReference == null)
        {
            documentReference = Document.Reference;
            document = Document;
            model = Document.Model;
        }
        else if (Document == null && DocumentReference != null)
        {
            documentReference = DocumentReference;
            document = null;
            model = null;
        }
        else
        {
            throw new ArgumentException($"Both {nameof(Document)} and {nameof(DocumentReference)} is a null reference. Provide at least one argument.");
        }

        JsonSerializerOptions jsonSerializerOptions = ConfigureJsonSerializerOption(JsonSerializerOptions);

        try
        {
            var response = await Execute(HttpMethod.Get, documentReference.BuildUrl(Config.ProjectId));
            using Stream contentStream = await response.Content.ReadAsStreamAsync();
            JsonDocument jsonDocument = await JsonDocument.ParseAsync(contentStream);
            var parsedDocument = ParseDocument(documentReference, model, document, jsonDocument.RootElement.EnumerateObject(), jsonSerializerOptions);

            return new(this, parsedDocument, null);
        }
        catch (Exception ex)
        {
            return new(this, null, ex);
        }
    }
}
