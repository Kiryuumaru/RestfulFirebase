using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using RestfulFirebase.FirestoreDatabase.References;
using RestfulFirebase.Common.Requests;
using System.Diagnostics.CodeAnalysis;
using RestfulFirebase.FirestoreDatabase.Models;
using System.Linq;
using RestfulFirebase.FirestoreDatabase.Transactions;

namespace RestfulFirebase.FirestoreDatabase.Requests;

/// <summary>
/// Request to get the <see cref="Document{T}"/> of the specified request query.
/// </summary>
/// <typeparam name="T">
/// The type of the model to populate the document fields.
/// </typeparam>
public class GetDocumentRequest<T> : FirestoreDatabaseRequest<TransactionResponse<GetDocumentRequest<T>, GetDocumentResult<T>>>
    where T : class
{
    /// <summary>
    /// Gets or sets the <see cref="JsonSerializerOptions"/> used to serialize and deserialize documents.
    /// </summary>
    public JsonSerializerOptions? JsonSerializerOptions { get; set; }

    /// <summary>
    /// Gets or sets the requested <see cref="Document{T}"/> documents.
    /// </summary>
    public Document<T>? Document { get; set; }

    /// <summary>
    /// Gets or sets the requested <see cref="Document{T}"/> documents.
    /// </summary>
    public IEnumerable<Document<T>>? Documents { get; set; }

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

    /// <inheritdoc cref="GetDocumentRequest{T}"/>
    /// <returns>
    /// The <see cref="Task"/> proxy that represents the <see cref="TransactionResponse"/> with the result <see cref="GetDocumentResult{T}"/>.
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
    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    internal override async Task<TransactionResponse<GetDocumentRequest<T>, GetDocumentResult<T>>> Execute()
    {
        ArgumentNullException.ThrowIfNull(Config);
        if (Document == null && Documents == null && DocumentReference == null && DocumentReferences == null)
        {
            throw new ArgumentException($"{nameof(Document)}, {nameof(Documents)}, {nameof(DocumentReference)} and {nameof(DocumentReferences)} are null references. Provide at least one argument.");
        }

        JsonSerializerOptions jsonSerializerOptions = ConfigureJsonSerializerOption(JsonSerializerOptions);

        try
        {
            using MemoryStream stream = new();
            Utf8JsonWriter writer = new(stream);

            writer.WriteStartObject();
            writer.WritePropertyName("documents");
            writer.WriteStartArray();
            if (Document != null)
            {
                writer.WriteStringValue(Document.Reference.BuildUrlCascade(Config.ProjectId));
            }
            if (DocumentReference != null)
            {
                writer.WriteStringValue(DocumentReference.BuildUrlCascade(Config.ProjectId));
            }
            if (Documents != null)
            {
                foreach (var document in Documents)
                {
                    writer.WriteStringValue(document.Reference.BuildUrlCascade(Config.ProjectId));
                }
            }
            if (DocumentReferences != null)
            {
                foreach (var reference in DocumentReferences)
                {
                    writer.WriteStringValue(reference.BuildUrlCascade(Config.ProjectId));
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

            var response = await ExecuteWithContent(stream, HttpMethod.Post, BuildUrl());
            using Stream contentStream = await response.Content.ReadAsStreamAsync();
            JsonDocument jsonDocument = await JsonDocument.ParseAsync(contentStream);

            List<DocumentTimestamp<T>> foundDocuments = new();
            List<DocumentReferenceTimestamp> missingDocuments = new();

            foreach (var doc in jsonDocument.RootElement.EnumerateArray())
            {
                if (doc.TryGetProperty("readTime", out JsonElement readTimeProperty) &&
                    readTimeProperty.GetDateTimeOffset() is DateTimeOffset readTime)
                {
                    DocumentReference? documentReference = null;
                    Document<T>? document = null;
                    T? model = null;
                    if (doc.TryGetProperty("found", out JsonElement foundProperty))
                    {
                        if (foundProperty.TryGetProperty("name", out JsonElement foundNameProperty) &&
                            ParseDocumentReference(foundNameProperty, jsonSerializerOptions) is DocumentReference docRef)
                        {
                            documentReference = docRef;

                            if (Document != null &&
                                Document.Reference.Equals(docRef))
                            {
                                document = Document;
                                model = Document.Model;
                            }
                            else if (Documents != null &&
                                Documents.FirstOrDefault(i => i.Reference.Equals(docRef)) is Document<T> foundDocument)
                            {
                                document = foundDocument;
                                model = foundDocument.Model;
                            }
                        }

                        if (ParseDocument(documentReference, model, document, foundProperty.EnumerateObject(), jsonSerializerOptions) is Document<T> found)
                        {
                            foundDocuments.Add(new DocumentTimestamp<T>(found, readTime));
                        }
                    }
                    else if (doc.TryGetProperty("missing", out JsonElement missingProperty) &&
                        ParseDocumentReference(missingProperty, jsonSerializerOptions) is DocumentReference missing)
                    {
                        missingDocuments.Add(new DocumentReferenceTimestamp(missing, readTime));
                    }
                }
            }

            return new(this, new GetDocumentResult<T>(foundDocuments.AsReadOnly(), missingDocuments.AsReadOnly()), null);
        }
        catch (Exception ex)
        {
            return new(this, null, ex);
        }
    }

    internal string BuildUrl()
    {
        ArgumentNullException.ThrowIfNull(Config);

        return
            $"{Api.FirestoreDatabase.FirestoreDatabaseV1Endpoint}/" +
            $"{string.Format(Api.FirestoreDatabase.FirestoreDatabaseDocumentsEndpoint, Config.ProjectId, ":batchGet")}";
    }
}

/// <summary>
/// The result of the <see cref="GetDocumentRequest{T}"/> request.
/// </summary>
/// <typeparam name="T">
/// The type of the model of the document.
/// </typeparam>
public class GetDocumentResult<T>
    where T : class
{
    /// <summary>
    /// Gets the found document.
    /// </summary>
    public IReadOnlyList<DocumentTimestamp<T>> Found { get; }

    /// <summary>
    /// Gets the missing document.
    /// </summary>
    public IReadOnlyList<DocumentReferenceTimestamp> Missing { get; }

    internal GetDocumentResult(IReadOnlyList<DocumentTimestamp<T>> found, IReadOnlyList<DocumentReferenceTimestamp> missing)
    {
        Found = found;
        Missing = missing;
    }
}
