using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using RestfulFirebase.FirestoreDatabase.References;
using System.Diagnostics.CodeAnalysis;
using RestfulFirebase.FirestoreDatabase.Models;
using System.Linq;
using RestfulFirebase.FirestoreDatabase.Transactions;
using System.Threading;
using RestfulFirebase.Common.Http;
using RestfulFirebase.Common.Abstractions;

namespace RestfulFirebase.FirestoreDatabase;

public partial class FirestoreDatabaseApi
{
    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    internal async Task<(JsonDocument?, HttpResponse)> ExecuteGetDocument(
        IEnumerable<Document> documents,
        Transaction? transaction,
        IAuthorization? authorization,
        CancellationToken cancellationToken)
    {
        string url =
            $"{FirestoreDatabaseV1Endpoint}/" +
            $"{string.Format(FirestoreDatabaseDocumentsEndpoint, App.Config.ProjectId, ":batchGet")}";

        using MemoryStream stream = new();
        Utf8JsonWriter writer = new(stream);

        writer.WriteStartObject();
        writer.WritePropertyName("documents");
        writer.WriteStartArray();
        foreach (var doc in documents)
        {
            writer.WriteStringValue(doc.Reference.BuildUrlCascade(App.Config.ProjectId));
        }
        writer.WriteEndArray();
        if (transaction != null)
        {
            if (transaction.Token == null)
            {
                writer.WritePropertyName("newTransaction");
                BuildTransactionOption(writer, transaction);
            }
            else
            {
                BuildTransaction(writer, transaction);
            }
        }
        writer.WriteEndObject();

        await writer.FlushAsync(cancellationToken);

        var response = await ExecutePost(authorization, stream, url, cancellationToken);
        if (response.IsError || response.HttpTransactions.LastOrDefault() is not HttpTransaction lastHttpTransaction)
        {
            return (null, response);
        }

#if NET6_0_OR_GREATER
        using Stream contentStream = await lastHttpTransaction.HttpResponseMessage.Content.ReadAsStreamAsync(cancellationToken);
#else
        using Stream contentStream = await lastHttpTransaction.HttpResponseMessage.Content.ReadAsStreamAsync();
#endif
        return (await JsonDocument.ParseAsync(contentStream, cancellationToken: cancellationToken), response);
    }

    /// <summary>
    /// Request to get the document of the specified request query.
    /// </summary>
    /// <param name="document">
    /// The single or multiple documents to get.
    /// </param>
    /// <param name="transaction">
    /// The <see cref="Transaction"/> to optionally perform an atomic operation.
    /// </param>
    /// <param name="jsonSerializerOptions">
    /// The <see cref="JsonSerializerOptions"/> used to serialize and deserialize documents.
    /// </param>
    /// <param name="authorization">
    /// The authorization used for the operation.
    /// </param>
    /// <param name="cancellationToken">
    /// The <see cref="CancellationToken"/> that propagates notification if the operations should be canceled.
    /// </param>
    /// <returns>
    /// The <see cref="Task"/> proxy that represents the <see cref="HttpResponse"/> with the result <see cref="GetDocumentResult"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="document"/> is a null reference.
    /// </exception>
    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    public async Task<HttpResponse<GetDocumentResult>> GetDocument(Document.Builder document, Transaction? transaction = default, IAuthorization? authorization = default, JsonSerializerOptions? jsonSerializerOptions = default, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(document);

        JsonSerializerOptions configuredJsonSerializerOptions = ConfigureJsonSerializerOption(jsonSerializerOptions);

        HttpResponse<GetDocumentResult> response = new();

        var (jsonDocument, getDocumentResponse) = await ExecuteGetDocument(document.Documents, transaction, authorization, cancellationToken);
        response.Concat(getDocumentResponse);
        if (getDocumentResponse.IsError || jsonDocument == null)
        {
            return response;
        }

        List<DocumentTimestamp> foundDocuments = new();
        List<DocumentReferenceTimestamp> missingDocuments = new();

        foreach (var doc in jsonDocument.RootElement.EnumerateArray())
        {
            if (doc.TryGetProperty("readTime", out JsonElement readTimeProperty) &&
                readTimeProperty.GetDateTimeOffset() is DateTimeOffset readTime)
            {
                DocumentReference? parsedDocumentReference = null;
                Document? parsedDocument = null;
                object? parsedModel = null;
                if (doc.TryGetProperty("found", out JsonElement foundProperty))
                {
                    if (foundProperty.TryGetProperty("name", out JsonElement foundNameProperty) &&
                        DocumentReference.Parse(App, foundNameProperty, configuredJsonSerializerOptions) is DocumentReference docRef)
                    {
                        parsedDocumentReference = docRef;

                        if (document.Documents.FirstOrDefault(i => i.Reference.Equals(docRef)) is Document foundDocument)
                        {
                            parsedDocument = foundDocument;
                            parsedModel = foundDocument.GetModel();
                        }
                    }

                    if (Document.Parse(App, parsedDocumentReference, parsedModel?.GetType(), parsedModel, parsedDocument, foundProperty.EnumerateObject(), configuredJsonSerializerOptions) is Document found)
                    {
                        foundDocuments.Add(new DocumentTimestamp(found, readTime));
                    }
                }
                else if (doc.TryGetProperty("missing", out JsonElement missingProperty) &&
                    DocumentReference.Parse(App, missingProperty, configuredJsonSerializerOptions) is DocumentReference missing)
                {
                    missingDocuments.Add(new DocumentReferenceTimestamp(missing, readTime));
                }
            }
            else if (transaction != null &&
                jsonDocument.RootElement.TryGetProperty("transaction", out JsonElement transactionElement) &&
                transactionElement.GetString() is string transactionToken)
            {
                transaction.Token = transactionToken;
            }
        }

        return response.Concat(new GetDocumentResult(foundDocuments.AsReadOnly(), missingDocuments.AsReadOnly()));
    }

    /// <summary>
    /// Request to get the document of the specified request query.
    /// </summary>
    /// <typeparam name="T">
    /// The type of the document model.
    /// </typeparam>
    /// <param name="document">
    /// The single or multiple documents to get.
    /// </param>
    /// <param name="transaction">
    /// The <see cref="Transaction"/> to optionally perform an atomic operation.
    /// </param>
    /// <param name="jsonSerializerOptions">
    /// The <see cref="JsonSerializerOptions"/> used to serialize and deserialize documents.
    /// </param>
    /// <param name="authorization">
    /// The authorization used for the operation.
    /// </param>
    /// <param name="cancellationToken">
    /// The <see cref="CancellationToken"/> that propagates notification if the operations should be canceled.
    /// </param>
    /// <returns>
    /// The <see cref="Task"/> proxy that represents the <see cref="HttpResponse"/> with the result <see cref="GetDocumentResult"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="document"/> is a null reference.
    /// </exception>
    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    public async Task<HttpResponse<GetDocumentResult<T>>> GetDocument<T>(Document<T>.Builder document, Transaction? transaction = default, IAuthorization? authorization = default, JsonSerializerOptions? jsonSerializerOptions = default, CancellationToken cancellationToken = default)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(document);

        JsonSerializerOptions configuredJsonSerializerOptions = ConfigureJsonSerializerOption(jsonSerializerOptions);

        HttpResponse<GetDocumentResult<T>> response = new();

        var (jsonDocument, getDocumentResponse) = await ExecuteGetDocument(document.Documents, transaction, authorization, cancellationToken);
        response.Concat(getDocumentResponse);
        if (getDocumentResponse.IsError || jsonDocument == null)
        {
            return response;
        }

        List<DocumentTimestamp<T>> foundDocuments = new();
        List<DocumentReferenceTimestamp> missingDocuments = new();

        foreach (var doc in jsonDocument.RootElement.EnumerateArray())
        {
            if (doc.TryGetProperty("readTime", out JsonElement readTimeProperty) &&
                readTimeProperty.GetDateTimeOffset() is DateTimeOffset readTime)
            {
                DocumentReference? parsedDocumentReference = null;
                Document<T>? parsedDocument = null;
                T? parsedModel = null;
                if (doc.TryGetProperty("found", out JsonElement foundProperty))
                {
                    if (foundProperty.TryGetProperty("name", out JsonElement foundNameProperty) &&
                        DocumentReference.Parse(App, foundNameProperty, configuredJsonSerializerOptions) is DocumentReference docRef)
                    {
                        parsedDocumentReference = docRef;

                        if (document.Documents.FirstOrDefault(i => i.Reference.Equals(docRef)) is Document<T> foundDocument)
                        {
                            parsedDocument = foundDocument;
                            parsedModel = foundDocument.Model;
                        }
                    }

                    if (Document<T>.Parse(App, parsedDocumentReference, parsedModel, parsedDocument, foundProperty.EnumerateObject(), configuredJsonSerializerOptions) is Document<T> found)
                    {
                        foundDocuments.Add(new DocumentTimestamp<T>(found, readTime));
                    }
                }
                else if (doc.TryGetProperty("missing", out JsonElement missingProperty) &&
                    DocumentReference.Parse(App, missingProperty, configuredJsonSerializerOptions) is DocumentReference missing)
                {
                    missingDocuments.Add(new DocumentReferenceTimestamp(missing, readTime));
                }
            }
            else if (transaction != null &&
                jsonDocument.RootElement.TryGetProperty("transaction", out JsonElement transactionElement) &&
                transactionElement.GetString() is string transactionToken)
            {
                transaction.Token = transactionToken;
            }
        }

        return response.Concat(new GetDocumentResult<T>(foundDocuments.AsReadOnly(), missingDocuments.AsReadOnly()));
    }
}
