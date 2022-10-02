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
using System.Threading;

namespace RestfulFirebase.FirestoreDatabase.Requests;

internal static class GetDocumentRequestHelpers
{
    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    public static async Task<object?> ExecuteQuery<TRequest, TResult>(
        BaseGetDocumentRequest<TRequest, TResult> request,
        FirebaseConfig config,
        IEnumerable<Document> documents,
        JsonSerializerOptions jsonSerializerOptions,
        CancellationToken cancellationToken)
        where TRequest : TransactionRequest
    {
        CancellationToken linkedCancellationToken = CancellationTokenSource.CreateLinkedTokenSource(request.CancellationToken, cancellationToken).Token;

        string url =
            $"{Api.FirestoreDatabase.FirestoreDatabaseV1Endpoint}/" +
            $"{string.Format(Api.FirestoreDatabase.FirestoreDatabaseDocumentsEndpoint, config.ProjectId, ":batchGet")}";

        using MemoryStream stream = new();
        Utf8JsonWriter writer = new(stream);

        writer.WriteStartObject();
        writer.WritePropertyName("documents");
        writer.WriteStartArray();
        foreach (var document in documents)
        {
            writer.WriteStringValue(document.Reference.BuildUrlCascade(config.ProjectId));
        }
        writer.WriteEndArray();
        if (request.Transaction != null)
        {
            if (request.Transaction.Transaction.Token == null)
            {
                writer.WritePropertyName("newTransaction");
                if (request.Transaction.Transaction is ReadOnlyTransaction readOnlyTransaction)
                {
                    writer.WriteStartObject();
                    writer.WritePropertyName("readOnly");
                    writer.WriteStartObject();
                    if (readOnlyTransaction.ReadTime.HasValue)
                    {
                        writer.WritePropertyName("readTime");
                        writer.WriteStringValue(readOnlyTransaction.ReadTime.Value.ToUniversalTime());
                    }
                    writer.WriteEndObject();
                    writer.WriteEndObject();
                }
                else if (request.Transaction.Transaction is ReadWriteTransaction readWriteTransaction)
                {
                    writer.WriteStartObject();
                    writer.WritePropertyName("readWrite");
                    writer.WriteStartObject();
                    if (readWriteTransaction.RetryTransaction != null)
                    {
                        writer.WritePropertyName("retryTransaction");
                        writer.WriteStringValue(readWriteTransaction.RetryTransaction);
                    }
                    writer.WriteEndObject();
                    writer.WriteEndObject();
                }
            }
            else
            {
                writer.WritePropertyName("transaction");
                writer.WriteStringValue(request.Transaction.Transaction.Token);
            }
        }
        writer.WriteEndObject();

        await writer.FlushAsync(linkedCancellationToken);

        var (executeResult, executeException) = await request.ExecuteWithContent(stream, HttpMethod.Post, url);
        if (executeResult == null)
        {
            return executeException;
        }

#if NET6_0_OR_GREATER
        using Stream contentStream = await executeResult.Content.ReadAsStreamAsync(linkedCancellationToken);
#else
        using Stream contentStream = await executeResult.Content.ReadAsStreamAsync();
#endif
        return await JsonDocument.ParseAsync(contentStream, cancellationToken: linkedCancellationToken);
    }
}

/// <summary>
/// Request to get the document of the specified request query.
/// </summary>
public abstract class BaseGetDocumentRequest<TRequest, TResult> : FirestoreDatabaseRequest<TransactionResponse<TRequest, TResult>>
    where TRequest : TransactionRequest
{
    /// <summary>
    /// Gets or sets the <see cref="System.Text.Json.JsonSerializerOptions"/> used to serialize and deserialize documents.
    /// </summary>
    public JsonSerializerOptions? JsonSerializerOptions { get; set; }

    /// <summary>
    /// Gets or sets the <see cref="Transaction.Builder"/> of the transaction.
    /// </summary>
    public Transaction.Builder? Transaction { get; set; }
}

/// <summary>
/// Request to get the <see cref="Document{T}"/> of the specified request query.
/// </summary>
public class GetDocumentRequest : BaseGetDocumentRequest<GetDocumentRequest, GetDocumentResult>
{
    /// <summary>
    /// Gets or sets the type of the document model.
    /// </summary>
    public Type? ModelType { get; set; }

    /// <summary>
    /// Gets or sets the requested <see cref="Document"/> documents to get and patch.
    /// </summary>
    public Document.Builder? Document { get; set; }

    /// <inheritdoc cref="GetDocumentRequest"/>
    /// <returns>
    /// The <see cref="Task"/> proxy that represents the <see cref="TransactionResponse"/> with the result <see cref="GetDocumentResult"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <see cref="TransactionRequest.Config"/> and
    /// <see cref="Document"/> is a null reference.
    /// </exception>
    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    internal override async Task<TransactionResponse<GetDocumentRequest, GetDocumentResult>> Execute()
    {
        ArgumentNullException.ThrowIfNull(Config);
        ArgumentNullException.ThrowIfNull(Document);

        JsonSerializerOptions jsonSerializerOptions = ConfigureJsonSerializerOption(JsonSerializerOptions);

        object? result = await GetDocumentRequestHelpers.ExecuteQuery(
            this,
            Config,
            Document.Documents,
            jsonSerializerOptions,
            default);

        if (result is not JsonDocument jsonDocument)
        {
            return new(this, null, result as Exception);
        }

        List<DocumentTimestamp> foundDocuments = new();
        List<DocumentReferenceTimestamp> missingDocuments = new();

        foreach (var doc in jsonDocument.RootElement.EnumerateArray())
        {
            if (doc.TryGetProperty("readTime", out JsonElement readTimeProperty) &&
                readTimeProperty.GetDateTimeOffset() is DateTimeOffset readTime)
            {
                DocumentReference? documentReference = null;
                Document? document = null;
                object? model = null;
                if (doc.TryGetProperty("found", out JsonElement foundProperty))
                {
                    if (foundProperty.TryGetProperty("name", out JsonElement foundNameProperty) &&
                        DocumentReference.Parse(foundNameProperty, jsonSerializerOptions) is DocumentReference docRef)
                    {
                        documentReference = docRef;

                        if (Document.Documents.FirstOrDefault(i => i.Reference.Equals(docRef)) is Document foundDocument)
                        {
                            document = foundDocument;
                            model = foundDocument.GetModel();
                        }
                    }

                    if (Models.Document.Parse(documentReference, ModelType ?? model?.GetType(), model, document, foundProperty.EnumerateObject(), jsonSerializerOptions) is Document found)
                    {
                        foundDocuments.Add(new DocumentTimestamp(found, readTime));
                    }
                }
                else if (doc.TryGetProperty("missing", out JsonElement missingProperty) &&
                    DocumentReference.Parse(missingProperty, jsonSerializerOptions) is DocumentReference missing)
                {
                    missingDocuments.Add(new DocumentReferenceTimestamp(missing, readTime));
                }
            }
            else if (Transaction != null &&
                jsonDocument.RootElement.TryGetProperty("transaction", out JsonElement transactionElement) &&
                transactionElement.GetString() is string transactionToken)
            {
                Transaction.Transaction.Token = transactionToken;
            }
        }

        return new(this, new GetDocumentResult(foundDocuments.AsReadOnly(), missingDocuments.AsReadOnly()), null);
    }
}

/// <summary>
/// The result of the <see cref="GetDocumentRequest"/> request.
/// </summary>
public class GetDocumentResult
{
    /// <summary>
    /// Gets the found document.
    /// </summary>
    public IReadOnlyList<DocumentTimestamp> Found { get; }

    /// <summary>
    /// Gets the missing document.
    /// </summary>
    public IReadOnlyList<DocumentReferenceTimestamp> Missing { get; }

    internal GetDocumentResult(IReadOnlyList<DocumentTimestamp> found, IReadOnlyList<DocumentReferenceTimestamp> missing)
    {
        Found = found;
        Missing = missing;
    }
}

/// <summary>
/// Request to get the <see cref="Document{T}"/> of the specified request query.
/// </summary>
/// <typeparam name="T">
/// The type of the model to populate the document fields.
/// </typeparam>
public class GetDocumentRequest<T> : BaseGetDocumentRequest<GetDocumentRequest<T>, GetDocumentResult<T>>
    where T : class
{
    /// <summary>
    /// Gets or sets the requested <see cref="Document{T}"/> documents to get and patch.
    /// </summary>
    public Document<T>.Builder? Document { get; set; }

    /// <inheritdoc cref="GetDocumentRequest{T}"/>
    /// <returns>
    /// The <see cref="Task"/> proxy that represents the <see cref="TransactionResponse"/> with the result <see cref="GetDocumentResult{T}"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <see cref="TransactionRequest.Config"/> and
    /// <see cref="Document"/> is a null reference.
    /// </exception>
    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    internal override async Task<TransactionResponse<GetDocumentRequest<T>, GetDocumentResult<T>>> Execute()
    {
        ArgumentNullException.ThrowIfNull(Config);
        ArgumentNullException.ThrowIfNull(Document);

        JsonSerializerOptions jsonSerializerOptions = ConfigureJsonSerializerOption(JsonSerializerOptions);

        object? result = await GetDocumentRequestHelpers.ExecuteQuery(
            this,
            Config,
            Document.Documents,
            jsonSerializerOptions,
            default);

        if (result is not JsonDocument jsonDocument)
        {
            return new(this, null, result as Exception);
        }

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
                        DocumentReference.Parse(foundNameProperty, jsonSerializerOptions) is DocumentReference docRef)
                    {
                        documentReference = docRef;

                        if (Document.Documents.FirstOrDefault(i => i.Reference.Equals(docRef)) is Document<T> foundDocument)
                        {
                            document = foundDocument;
                            model = foundDocument.Model;
                        }
                    }

                    if (Document<T>.Parse(documentReference, model, document, foundProperty.EnumerateObject(), jsonSerializerOptions) is Document<T> found)
                    {
                        foundDocuments.Add(new DocumentTimestamp<T>(found, readTime));
                    }
                }
                else if (doc.TryGetProperty("missing", out JsonElement missingProperty) &&
                    DocumentReference.Parse(missingProperty, jsonSerializerOptions) is DocumentReference missing)
                {
                    missingDocuments.Add(new DocumentReferenceTimestamp(missing, readTime));
                }
            }
            else if (Transaction != null &&
                jsonDocument.RootElement.TryGetProperty("transaction", out JsonElement transactionElement) &&
                transactionElement.GetString() is string transactionToken)
            {
                Transaction.Transaction.Token = transactionToken;
            }
        }

        return new(this, new GetDocumentResult<T>(foundDocuments.AsReadOnly(), missingDocuments.AsReadOnly()), null);
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
