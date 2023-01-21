using RestfulFirebase.Common.Abstractions;
using RestfulFirebase.Common.Http;
using RestfulFirebase.FirestoreDatabase.Models;
using RestfulFirebase.FirestoreDatabase.References;
using RestfulFirebase.FirestoreDatabase.Transactions;
using RestfulFirebase.FirestoreDatabase.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace RestfulFirebase.FirestoreDatabase.Fetches;

public abstract partial class FluentFetchRoot<TFetch>
{
    /// <summary>
    /// Adds a document references to fetch.
    /// </summary>
    /// <param name="documentReferences">
    /// The document references to fetch.
    /// </param>
    /// <returns>
    /// The fetch with new added document references.
    /// </returns>
    public TFetch DocumentReference(params DocumentReference[] documentReferences)
    {
        TFetch fetch = (TFetch)Clone();

        if (documentReferences != null)
        {
            fetch.WritableDocumentReferences.AddRange(documentReferences);
        }

        return fetch;
    }

    /// <summary>
    /// Adds a document references to fetch.
    /// </summary>
    /// <param name="documentReferences">
    /// The document references to fetch.
    /// </param>
    /// <returns>
    /// The fetch with new added documents references.
    /// </returns>
    public TFetch DocumentReference(IEnumerable<DocumentReference> documentReferences)
    {
        TFetch fetch = (TFetch)Clone();

        if (documentReferences != null)
        {
            fetch.WritableDocumentReferences.AddRange(documentReferences);
        }

        return fetch;
    }

    /// <summary>
    /// Adds a documents to fetch.
    /// </summary>
    /// <param name="documents">
    /// The documents to fetch.
    /// </param>
    /// <returns>
    /// The fetch with new added documents.
    /// </returns>
    public TFetch Document(params Document[] documents)
    {
        TFetch fetch = (TFetch)Clone();

        if (documents != null)
        {
            fetch.WritableDocuments.AddRange(documents);
        }

        return fetch;
    }

    /// <summary>
    /// Adds a documents to fetch.
    /// </summary>
    /// <param name="documents">
    /// The documents to fetch.
    /// </param>
    /// <returns>
    /// The fetch with new added documents.
    /// </returns>
    public TFetch Document(IEnumerable<Document> documents)
    {
        TFetch fetch = (TFetch)Clone();

        if (documents != null)
        {
            fetch.WritableDocuments.AddRange(documents);
        }

        return fetch;
    }

    /// <summary>
    /// Adds a cache documents.
    /// </summary>
    /// <param name="documents">
    /// The cache documents.
    /// </param>
    /// <returns>
    /// The request with new added cache documents.
    /// </returns>
    public TFetch Cache(params Document[] documents)
    {
        TFetch fetch = (TFetch)Clone();

        if (documents != null)
        {
            fetch.WritableCacheDocuments.AddRange(documents);
        }

        return fetch;
    }

    /// <summary>
    /// Adds a cache documents.
    /// </summary>
    /// <param name="documents">
    /// The cache documents.
    /// </param>
    /// <returns>
    /// The request with new added cache documents.
    /// </returns>
    public TFetch Cache(IEnumerable<Document>? documents)
    {
        TFetch fetch = (TFetch)Clone();

        if (documents != null)
        {
            fetch.WritableCacheDocuments.AddRange(documents);
        }

        return fetch;
    }

    /// <summary>
    /// Sets the <see cref="Transactions.Transaction"/> to optionally perform an atomic operation.
    /// </summary>
    /// <returns>
    /// The request with new added transaction.
    /// </returns>
    public TFetch Transaction(Transaction? transaction)
    {
        TFetch fetch = (TFetch)Clone();

        fetch.TransactionUsed = transaction;

        return fetch;
    }

    /// <summary>
    /// Sets the <see cref="Fetch.AuthorizationUsed"/> by the request.
    /// </summary>
    /// <returns>
    /// The request with new added authorization.
    /// </returns>
    public TFetch Authorization(IAuthorization? authorization)
    {
        TFetch fetch = (TFetch)Clone();

        fetch.AuthorizationUsed = authorization;

        return fetch;
    }

    /// <summary>
    /// Runs the structured fetch.
    /// </summary>
    /// <param name="cancellationToken">
    /// The <see cref="CancellationToken"/> that propagates notification if the operations should be canceled.
    /// </param>
    /// <returns>
    /// The <see cref="Task"/> proxy that represents the <see cref="HttpResponse"/> with the created result <see cref="GetDocumentsResult"/>.
    /// </returns>
    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    public async Task<HttpResponse<GetDocumentsResult>> Run(CancellationToken cancellationToken = default)
    {
        JsonSerializerOptions jsonSerializerOptions = App.FirestoreDatabase.ConfigureJsonSerializerOption();

        List<DocumentReference> allDocRefs = new(DocumentReferences);
        foreach (var doc in Documents)
        {
            if (!allDocRefs.Contains(doc.Reference))
            {
                allDocRefs.Add(doc.Reference);
            }
        }

        List<Document> allDocs = new(CacheDocuments);
        foreach (var doc in Documents)
        {
            if (!allDocs.Any(i => i.Reference.Equals(doc.Reference)))
            {
                allDocs.Add(doc);
            }
        }
        HttpResponse<GetDocumentsResult> response = new();

        var (jsonDocument, getDocumentResponse) = await ExecuteGetDocument(allDocRefs, TransactionUsed, AuthorizationUsed, cancellationToken);
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
                        References.DocumentReference.Parse(App, foundNameProperty, jsonSerializerOptions) is DocumentReference docRef)
                    {
                        parsedDocumentReference = docRef;

                        if (allDocs?.FirstOrDefault(i => i.Reference.Equals(docRef)) is Document foundDocument)
                        {
                            parsedDocument = foundDocument;
                            parsedModel = foundDocument.GetModel();
                        }
                    }

                    if (ModelBuilderHelpers.Parse(App, parsedDocumentReference, parsedDocument?.Type, parsedModel, parsedDocument, foundProperty.EnumerateObject(), jsonSerializerOptions) is Document found)
                    {
                        foundDocuments.Add(new DocumentTimestamp(found, readTime, true));
                    }
                }
                else if (doc.TryGetProperty("missing", out JsonElement missingProperty) &&
                    References.DocumentReference.Parse(App, missingProperty, jsonSerializerOptions) is DocumentReference missing)
                {
                    missingDocuments.Add(new DocumentReferenceTimestamp(missing, readTime, true));
                }
            }
            else if (TransactionUsed != null &&
                jsonDocument.RootElement.TryGetProperty("transaction", out JsonElement transactionElement) &&
                transactionElement.GetString() is string transactionToken)
            {
                TransactionUsed.Token = transactionToken;
            }
        }

        response.Append(new GetDocumentsResult(foundDocuments.AsReadOnly(), missingDocuments.AsReadOnly()));

        return response;
    }

    /// <summary>
    /// Runs the structured fetch.
    /// </summary>
    /// <param name="cancellationToken">
    /// The <see cref="CancellationToken"/> that propagates notification if the operations should be canceled.
    /// </param>
    /// <returns>
    /// The <see cref="Task"/> proxy that represents the <see cref="HttpResponse"/> with the created result <see cref="GetDocumentsResult"/>.
    /// </returns>
    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    public async Task<HttpResponse<GetDocumentResult>> RunSingle(CancellationToken cancellationToken = default)
    {
        if (Documents.Count + DocumentReferences.Count != 1)
        {
            ArgumentException.Throw($"Fetch operation has multiple or empty document to execute.");
        }

        HttpResponse<GetDocumentResult> response = new();

        var runResponse = await Run(cancellationToken);
        response.Append(runResponse);
        if (runResponse.IsError)
        {
            return response;
        }

        response.Append(new GetDocumentResult(runResponse.Result?.Found.FirstOrDefault(), runResponse.Result?.Missing?.FirstOrDefault()));

        return response;
    }
}

public abstract partial class FluentFetchRoot<TFetch, TModel>
{
    /// <summary>
    /// Runs the structured fetch.
    /// </summary>
    /// <param name="cancellationToken">
    /// The <see cref="CancellationToken"/> that propagates notification if the operations should be canceled.
    /// </param>
    /// <returns>
    /// The <see cref="Task"/> proxy that represents the <see cref="HttpResponse"/> with the created result <see cref="GetDocumentsResult{TModel}"/>.
    /// </returns>
    /// <exception cref="System.ArgumentException">
    /// Fetch operation has multiple or empty document to execute.
    /// </exception>
    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    public new async Task<HttpResponse<GetDocumentsResult<TModel>>> Run(CancellationToken cancellationToken = default)
    {
        JsonSerializerOptions jsonSerializerOptions = App.FirestoreDatabase.ConfigureJsonSerializerOption();

        List<DocumentReference> allDocRefs = new(DocumentReferences);
        foreach (var doc in Documents)
        {
            if (!allDocRefs.Contains(doc.Reference))
            {
                allDocRefs.Add(doc.Reference);
            }
        }

        List<Document> allDocs = new(CacheDocuments);
        foreach (var doc in Documents)
        {
            if (!allDocs.Any(i => i.Reference.Equals(doc.Reference)))
            {
                allDocs.Add(doc);
            }
        }

        HttpResponse<GetDocumentsResult<TModel>> response = new();

        var (jsonDocument, getDocumentResponse) = await ExecuteGetDocument(allDocRefs, TransactionUsed, AuthorizationUsed, cancellationToken);
        response.Append(getDocumentResponse);
        if (getDocumentResponse.IsError || jsonDocument == null)
        {
            return response;
        }

        List<DocumentTimestamp<TModel>> foundDocuments = new();
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
                        References.DocumentReference.Parse(App, foundNameProperty, jsonSerializerOptions) is DocumentReference docRef)
                    {
                        parsedDocumentReference = docRef;

                        if (allDocs?.FirstOrDefault(i => i.Reference.Equals(docRef)) is Document foundTypedDocument)
                        {
                            parsedDocument = foundTypedDocument;
                            parsedModel = foundTypedDocument.GetModel();
                        }
                    }

                    if (ModelBuilderHelpers.Parse<TModel>(App, parsedDocumentReference, parsedModel, parsedDocument, foundProperty.EnumerateObject(), jsonSerializerOptions) is Document<TModel> found)
                    {
                        foundDocuments.Add(new DocumentTimestamp<TModel>(found, readTime, true));
                    }
                }
                else if (doc.TryGetProperty("missing", out JsonElement missingProperty) &&
                    References.DocumentReference.Parse(App, missingProperty, jsonSerializerOptions) is DocumentReference missing)
                {
                    missingDocuments.Add(new DocumentReferenceTimestamp(missing, readTime, true));
                }
            }
            else if (TransactionUsed != null &&
                jsonDocument.RootElement.TryGetProperty("transaction", out JsonElement transactionElement) &&
                transactionElement.GetString() is string transactionToken)
            {
                TransactionUsed.Token = transactionToken;
            }
        }

        response.Append(new GetDocumentsResult<TModel>(foundDocuments.AsReadOnly(), missingDocuments.AsReadOnly()));

        return response;
    }

    /// <summary>
    /// Runs the structured fetch.
    /// </summary>
    /// <param name="cancellationToken">
    /// The <see cref="CancellationToken"/> that propagates notification if the operations should be canceled.
    /// </param>
    /// <returns>
    /// The <see cref="Task"/> proxy that represents the <see cref="HttpResponse"/> with the created result <see cref="GetDocumentsResult"/>.
    /// </returns>
    /// <exception cref="System.ArgumentException">
    /// Fetch operation has multiple or empty document to execute.
    /// </exception>
    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    public new async Task<HttpResponse<GetDocumentResult<TModel>>> RunSingle(CancellationToken cancellationToken = default)
    {
        if (Documents.Count + DocumentReferences.Count != 1)
        {
            ArgumentException.Throw($"Fetch operation has multiple or empty document to execute.");
        }

        HttpResponse<GetDocumentResult<TModel>> response = new();

        var runResponse = await Run(cancellationToken);
        response.Append(runResponse);
        if (runResponse.IsError)
        {
            return response;
        }

        response.Append(new GetDocumentResult<TModel>(runResponse.Result?.Found.FirstOrDefault(), runResponse.Result?.Missing?.FirstOrDefault()));

        return response;
    }
}
