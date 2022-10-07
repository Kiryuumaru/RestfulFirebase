﻿using System;
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
    internal async Task<HttpResponse<GetDocumentsResult>> GetDocument(IEnumerable<DocumentReference> documentReferences, IEnumerable<Document> documents, Transaction? transaction = default, IAuthorization? authorization = default, CancellationToken cancellationToken = default)
    {
        JsonSerializerOptions jsonSerializerOptions = ConfigureJsonSerializerOption();

        List<DocumentReference> allDocRefs = new(documentReferences);
        foreach (var doc in documents)
        {
            if (!allDocRefs.Contains(doc.Reference))
            {
                allDocRefs.Add(doc.Reference);
            }
        }

        HttpResponse<GetDocumentsResult> response = new();

        var (jsonDocument, getDocumentResponse) = await ExecuteGetDocument(allDocRefs, transaction, authorization, cancellationToken);
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
                        DocumentReference.Parse(App, foundNameProperty, jsonSerializerOptions) is DocumentReference docRef)
                    {
                        parsedDocumentReference = docRef;

                        if (documents.FirstOrDefault(i => i.Reference.Equals(docRef)) is Document foundDocument)
                        {
                            parsedDocument = foundDocument;
                            parsedModel = foundDocument.GetModel();
                        }
                    }

                    if (Document.Parse(App, parsedDocumentReference, parsedModel?.GetType(), parsedModel, parsedDocument, foundProperty.EnumerateObject(), jsonSerializerOptions) is Document found)
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
    internal async Task<HttpResponse<GetDocumentsResult<T>>> GetDocument<T>(IEnumerable<DocumentReference> documentReferences, IEnumerable<Document<T>> documents, Transaction? transaction = default, IAuthorization? authorization = default, CancellationToken cancellationToken = default)
        where T : class
    {
        JsonSerializerOptions jsonSerializerOptions = ConfigureJsonSerializerOption();

        List<DocumentReference> allDocRefs = new(documentReferences);
        foreach (var doc in documents)
        {
            if (!allDocRefs.Contains(doc.Reference))
            {
                allDocRefs.Add(doc.Reference);
            }
        }

        HttpResponse<GetDocumentsResult<T>> response = new();

        var (jsonDocument, getDocumentResponse) = await ExecuteGetDocument(allDocRefs, transaction, authorization, cancellationToken);
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
                        DocumentReference.Parse(App, foundNameProperty, jsonSerializerOptions) is DocumentReference docRef)
                    {
                        parsedDocumentReference = docRef;

                        if (documents.FirstOrDefault(i => i.Reference.Equals(docRef)) is Document<T> foundDocument)
                        {
                            parsedDocument = foundDocument;
                            parsedModel = foundDocument.Model;
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

    /// <summary>
    /// Request to get the document.
    /// </summary>
    /// <param name="document">
    /// The single or multiple documents to get.
    /// </param>
    /// <param name="transaction">
    /// The <see cref="Transaction"/> to optionally perform an atomic operation.
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
    public async Task<HttpResponse<GetDocumentResult>> GetDocument(Document document, Transaction? transaction = default, IAuthorization? authorization = default, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(document);

        HttpResponse<GetDocumentResult> response = new();

        var getDocumentResponse = await GetDocument(Array.Empty<DocumentReference>(), new Document[] { document }, transaction, authorization, cancellationToken);
        response.Concat(getDocumentResponse);
        if (getDocumentResponse.IsError)
        {
            return response;
        }

        var found = getDocumentResponse.Result.Found.FirstOrDefault();
        var missing = getDocumentResponse.Result.Missing.FirstOrDefault();

        response.Append(new GetDocumentResult(found, missing));

        return response;
    }

    /// <summary>
    /// Request to get the documents.
    /// </summary>
    /// <param name="documents">
    /// The single or multiple documents to get.
    /// </param>
    /// <param name="transaction">
    /// The <see cref="Transaction"/> to optionally perform an atomic operation.
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
    /// <paramref name="documents"/> is a null reference.
    /// </exception>
    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    public async Task<HttpResponse<GetDocumentsResult>> GetDocuments(IEnumerable<Document> documents, Transaction? transaction = default, IAuthorization? authorization = default, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(documents);

        HttpResponse<GetDocumentsResult> response = new();

        var getDocumentResponse = await GetDocument(Array.Empty<DocumentReference>(), documents, transaction, authorization, cancellationToken);
        response.Concat(getDocumentResponse);
        if (getDocumentResponse.IsError)
        {
            return response;
        }

        return response;
    }

    /// <summary>
    /// Request to get the document.
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
    public async Task<HttpResponse<GetDocumentResult<T>>> GetDocument<T>(Document<T> document, Transaction? transaction = default, IAuthorization? authorization = default, CancellationToken cancellationToken = default)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(document);

        HttpResponse<GetDocumentResult<T>> response = new();

        var getDocumentResponse = await GetDocument(Array.Empty<DocumentReference>(), new Document<T>[] { document }, transaction, authorization, cancellationToken);
        response.Concat(getDocumentResponse);
        if (getDocumentResponse.IsError)
        {
            return response;
        }

        var found = getDocumentResponse.Result.Found.FirstOrDefault();
        var missing = getDocumentResponse.Result.Missing.FirstOrDefault();

        response.Append(new GetDocumentResult<T>(found, missing));

        return response;
    }

    /// <summary>
    /// Request to get the documents.
    /// </summary>
    /// <typeparam name="T">
    /// The type of the document model.
    /// </typeparam>
    /// <param name="documents">
    /// The single or multiple documents to get.
    /// </param>
    /// <param name="transaction">
    /// The <see cref="Transaction"/> to optionally perform an atomic operation.
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
    /// <paramref name="documents"/> is a null reference.
    /// </exception>
    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    public async Task<HttpResponse<GetDocumentsResult<T>>> GetDocuments<T>(IEnumerable<Document<T>> documents, Transaction? transaction = default, IAuthorization? authorization = default, CancellationToken cancellationToken = default)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(documents);

        HttpResponse<GetDocumentsResult<T>> response = new();

        var getDocumentResponse = await GetDocument(Array.Empty<DocumentReference>(), documents, transaction, authorization, cancellationToken);
        response.Concat(getDocumentResponse);
        if (getDocumentResponse.IsError)
        {
            return response;
        }

        return response;
    }
}