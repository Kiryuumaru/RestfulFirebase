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
using System.Xml.Linq;

namespace RestfulFirebase.FirestoreDatabase;

public partial class FirestoreDatabaseApi
{
    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    internal async Task<(JsonDocument?, HttpResponse)> ExecuteGetDocument(
        IEnumerable<DocumentReference> documentReferences,
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
        foreach (var documentReference in documentReferences)
        {
            writer.WriteStringValue(documentReference.BuildUrlCascade(App.Config.ProjectId));
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
        using Stream contentStream = await lastHttpTransaction.ResponseMessage.Content.ReadAsStreamAsync(cancellationToken);
#else
        using Stream contentStream = await lastHttpTransaction.ResponseMessage.Content.ReadAsStreamAsync();
#endif
        return (await JsonDocument.ParseAsync(contentStream, cancellationToken: cancellationToken), response);
    }

    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    internal async Task<HttpResponse<GetDocumentsResult>> GetDocument(IEnumerable<DocumentReference>? documentReferences, IEnumerable<Document>? documents, IEnumerable<Document>? cacheDocuments, Transaction? transaction = default, IAuthorization? authorization = default, CancellationToken cancellationToken = default)
    {
        JsonSerializerOptions jsonSerializerOptions = ConfigureJsonSerializerOption();

        List<DocumentReference> allDocRefs = documentReferences == null ? new() : new(documentReferences);
        if (documents != null)
        {
            foreach (var doc in documents)
            {
                if (!allDocRefs.Contains(doc.Reference))
                {
                    allDocRefs.Add(doc.Reference);
                }
            }
        }

        List<Document> allDocs = cacheDocuments == null ? new() : new(cacheDocuments);
        if (documents != null)
        {
            foreach (var doc in documents)
            {
                if (!allDocs.Contains(doc))
                {
                    allDocs.Add(doc);
                }
            }
        }

        HttpResponse<GetDocumentsResult> response = new();

        var (jsonDocument, getDocumentResponse) = await ExecuteGetDocument(allDocRefs, transaction, authorization, cancellationToken);
        response.Append(getDocumentResponse);
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
                        DocumentReference.Parse(App, foundNameProperty, jsonSerializerOptions) is DocumentReference docRef)
                    {
                        parsedDocumentReference = docRef;

                        if (allDocs?.FirstOrDefault(i => i.Reference.Equals(docRef)) is Document foundDocument)
                        {
                            parsedDocument = foundDocument;
                            parsedModel = foundDocument.GetModel();
                        }
                    }

                    if (Document.Parse(App, parsedDocumentReference, parsedDocument?.Type, parsedModel, parsedDocument, foundProperty.EnumerateObject(), jsonSerializerOptions) is Document found)
                    {
                        foundDocuments.Add(new DocumentTimestamp(found, readTime));
                    }
                }
                else if (doc.TryGetProperty("missing", out JsonElement missingProperty) &&
                    DocumentReference.Parse(App, missingProperty, jsonSerializerOptions) is DocumentReference missing)
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

        return response.Append(new GetDocumentsResult(foundDocuments.AsReadOnly(), missingDocuments.AsReadOnly()));
    }

    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    internal async Task<HttpResponse<GetDocumentsResult<T>>> GetDocument<T>(IEnumerable<DocumentReference>? documentReferences, IEnumerable<Document>? documents, IEnumerable<Document<T>>? typedDocuments, IEnumerable<Document>? cacheDocuments, Transaction? transaction = default, IAuthorization? authorization = default, CancellationToken cancellationToken = default)
        where T : class
    {
        JsonSerializerOptions jsonSerializerOptions = ConfigureJsonSerializerOption();

        List<DocumentReference> allDocRefs = documentReferences == null ? new() : new(documentReferences);
        if (documents != null)
        {
            foreach (var doc in documents)
            {
                if (!allDocRefs.Contains(doc.Reference))
                {
                    allDocRefs.Add(doc.Reference);
                }
            }
        }
        if (typedDocuments != null)
        {
            foreach (var doc in typedDocuments)
            {
                if (!allDocRefs.Contains(doc.Reference))
                {
                    allDocRefs.Add(doc.Reference);
                }
            }
        }

        List<Document> allDocs = cacheDocuments == null ? new() : new(cacheDocuments);
        if (documents != null)
        {
            foreach (var doc in documents)
            {
                if (!allDocs.Contains(doc))
                {
                    allDocs.Add(doc);
                }
            }
        }
        if (typedDocuments != null)
        {
            foreach (var doc in typedDocuments)
            {
                if (!allDocs.Contains(doc))
                {
                    allDocs.Add(doc);
                }
            }
        }

        HttpResponse<GetDocumentsResult<T>> response = new();

        var (jsonDocument, getDocumentResponse) = await ExecuteGetDocument(allDocRefs, transaction, authorization, cancellationToken);
        response.Append(getDocumentResponse);
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
                Document? parsedDocument = null;
                object? parsedModel = null;
                if (doc.TryGetProperty("found", out JsonElement foundProperty))
                {
                    if (foundProperty.TryGetProperty("name", out JsonElement foundNameProperty) &&
                        DocumentReference.Parse(App, foundNameProperty, jsonSerializerOptions) is DocumentReference docRef)
                    {
                        parsedDocumentReference = docRef;

                        if (allDocs?.FirstOrDefault(i => i.Reference.Equals(docRef)) is Document foundTypedDocument)
                        {
                            parsedDocument = foundTypedDocument;
                            parsedModel = foundTypedDocument.GetModel();
                        }
                    }

                    if (Document<T>.Parse(App, parsedDocumentReference, parsedModel, parsedDocument, foundProperty.EnumerateObject(), jsonSerializerOptions) is Document<T> found)
                    {
                        foundDocuments.Add(new DocumentTimestamp<T>(found, readTime));
                    }
                }
                else if (doc.TryGetProperty("missing", out JsonElement missingProperty) &&
                    DocumentReference.Parse(App, missingProperty, jsonSerializerOptions) is DocumentReference missing)
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

        return response.Append(new GetDocumentsResult<T>(foundDocuments.AsReadOnly(), missingDocuments.AsReadOnly()));
    }
}
