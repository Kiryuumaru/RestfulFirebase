using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using RestfulFirebase.FirestoreDatabase.Queries;
using RestfulFirebase.FirestoreDatabase;
using System.Transactions;
using RestfulFirebase.Common.Transactions;
using System.Diagnostics.CodeAnalysis;
using RestfulFirebase.Common;
using RestfulFirebase.FirestoreDatabase.Models;
using System.Reflection;
using System.Linq;

namespace RestfulFirebase.FirestoreDatabase.Transactions;

/// <summary>
/// Request to get the multiple <see cref="Document{T}"/> of the specified request query.
/// </summary>
/// <typeparam name="T">
/// The type of the model to populate the document fields.
/// </typeparam>
public class GetDocumentsRequest<T> : FirestoreDatabaseRequest<TransactionResponse<GetDocumentsRequest<T>, BatchGetDocuments<T>>>
    where T : class
{
    /// <summary>
    /// Gets or sets the <see cref="JsonSerializerOptions"/> used to serialize and deserialize documents.
    /// </summary>
    public JsonSerializerOptions? JsonSerializerOptions { get; set; }

    /// <summary>
    /// Gets or sets the existing <see cref="Document{T}"/> to populate the document fields.
    /// </summary>
    public IEnumerable<Document<T>>? Documents { get; set; }

    /// <summary>
    /// Gets or sets the requested <see cref="DocumentReference"/> of the document node.
    /// </summary>
    public MultipleDocumentReferences? Reference
    {
        get => Query as MultipleDocumentReferences;
        set => Query = value;
    }

    /// <inheritdoc cref="GetDocumentsRequest{T}"/>
    /// <returns>
    /// The <see cref="Task"/> proxy that represents the <see cref="TransactionResponse"/> with the result <see cref="BatchGetDocuments{T}"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <see cref="TransactionRequest.Config"/> is a null reference.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <see cref="Documents"/> and <see cref="Reference"/> is a null reference.
    /// </exception>
    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    internal override async Task<TransactionResponse<GetDocumentsRequest<T>, BatchGetDocuments<T>>> Execute()
    {
        ArgumentNullException.ThrowIfNull(Config);
        if (Documents == null && Reference == null)
        {
            throw new ArgumentException($"Both {nameof(Documents)} and {nameof(Reference)} is a null reference. Provide at least one to get.");
        }

        JsonSerializerOptions jsonSerializerOptions = ConfigureJsonSerializerOption(JsonSerializerOptions);

        try
        {
            using MemoryStream stream = new();
            Utf8JsonWriter writer = new(stream);

            writer.WriteStartObject();
            writer.WritePropertyName("documents");
            writer.WriteStartArray();
            if (Documents != null)
            {
                foreach (var document in Documents)
                {
                    writer.WriteStringValue(document.Reference.BuildUrlCascade(Config.ProjectId));
                }
            }
            else if (Reference != null)
            {
                foreach (var reference in Reference.DocumentReferences)
                {
                    writer.WriteStringValue(reference.BuildUrlCascade(Config.ProjectId));
                }
            }
            writer.WriteEndArray();
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
                    Document<T>? document = null;
                    if (doc.TryGetProperty("found", out JsonElement foundProperty))
                    {
                        if (Documents != null &&
                            Documents.Count() != 0 &&
                            foundProperty.TryGetProperty("name", out JsonElement foundNameProperty) &&
                            foundNameProperty.ValueKind == JsonValueKind.String &&
                            foundNameProperty.GetString() is string foundName &&
                            Documents.FirstOrDefault(i => i.Name == foundName) is Document<T> foundDocument)
                        {
                            document = foundDocument;
                        }

                        if (ParseDocument(null, document, foundProperty.EnumerateObject(), jsonSerializerOptions) is Document<T> found)
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

            return new(this, new BatchGetDocuments<T>(foundDocuments.AsReadOnly(), missingDocuments.AsReadOnly()), null);
        }
        catch (Exception ex)
        {
            return new(this, null, ex);
        }
    }

    internal override string BuildUrl()
    {
        ArgumentNullException.ThrowIfNull(Config);

        return GetQuery().BuildUrl(Config.ProjectId, ":batchGet");
    }
}

/// <summary>
/// The result of the <see cref="GetDocumentsRequest{T}"/> request.
/// </summary>
/// <typeparam name="T">
/// The type of the model of the document.
/// </typeparam>
public class BatchGetDocuments<T>
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

    internal BatchGetDocuments(IReadOnlyList<DocumentTimestamp<T>> found, IReadOnlyList<DocumentReferenceTimestamp> missing)
    {
        Found = found;
        Missing = missing;
    }
}
