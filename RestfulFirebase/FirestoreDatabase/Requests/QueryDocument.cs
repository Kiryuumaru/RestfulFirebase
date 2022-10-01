using System;
using RestfulFirebase.Common.Requests;
using System.Threading.Tasks;
using System.Net.Http;
using System.IO;
using System.Text.Json;
using RestfulFirebase.FirestoreDatabase.Transactions;
using System.Diagnostics.CodeAnalysis;
using RestfulFirebase.FirestoreDatabase.Queries;
using RestfulFirebase.Common.Utilities;
using System.Reflection;
using RestfulFirebase.Common.Attributes;
using System.Collections.Generic;
using RestfulFirebase.FirestoreDatabase.References;
using System.Linq;
using RestfulFirebase.FirestoreDatabase.Models;
using RestfulFirebase.FirestoreDatabase.Enums;
using RestfulFirebase.FirestoreDatabase.Utilities;
using System.Data;
using System.Runtime.Serialization.Formatters;
using System.Threading;

namespace RestfulFirebase.FirestoreDatabase.Requests;

internal static class QueryDocumentRequestHelpers
{
    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    public static async Task<object?> ExecuteQuery<TRequest, TResult>(
        BaseQueryDocumentRequest<TRequest, TResult> request,
        int page,
        int actualPageSize,
        FirebaseConfig config,
        FromQuery.Builder fromQuery,
        FromQuery firstFromQuery,
        JsonSerializerOptions jsonSerializerOptions,
        Func<string, string> fieldNameFactory,
        CancellationToken cancellationToken)
        where TRequest : TransactionRequest
    {
        CancellationToken linkedCancellationToken = CancellationTokenSource.CreateLinkedTokenSource(request.CancellationToken, cancellationToken).Token;

        int limit = actualPageSize;
        int offset = page * limit;

        string url;
        if (request.DocumentReference != null)
        {
            if (fromQuery.FromQuery.Any(i => i.CollectionReference.Parent != null && i.CollectionReference.Parent != request.DocumentReference))
            {
                throw new ArgumentException($"\"{nameof(request.DocumentReference)}\" is provided but one or more \"{nameof(request.From)}\" has different parent document.");
            }

            url = request.DocumentReference.BuildUrl(config.ProjectId, ":runQuery");
        }
        else
        {
            if (fromQuery.FromQuery.Count == 1 &&
                firstFromQuery.CollectionReference.Parent != null)
            {
                url = firstFromQuery.CollectionReference.Parent.BuildUrl(config.ProjectId, ":runQuery");
            }
            else
            {
                if (fromQuery.FromQuery.Any(i => i.CollectionReference.Parent != firstFromQuery.CollectionReference.Parent))
                {
                    throw new ArgumentException($"\"{nameof(fromQuery)}\" has different parent document.");
                }

                url =
                    $"{Api.FirestoreDatabase.FirestoreDatabaseV1Endpoint}/" +
                    $"{string.Format(Api.FirestoreDatabase.FirestoreDatabaseDocumentsEndpoint, config.ProjectId, ":runQuery")}";
            }
        }

        using MemoryStream stream = new();
        Utf8JsonWriter writer = new(stream);

        writer.WriteStartObject();
        writer.WritePropertyName("structuredQuery");
        writer.WriteStartObject();
        writer.WritePropertyName("from");
        writer.WriteStartArray();
        foreach (var from in fromQuery.FromQuery)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("collectionId");
            writer.WriteStringValue(from.CollectionReference.Id);
            writer.WritePropertyName("allDescendants");
            writer.WriteBooleanValue(from.AllDescendants);
            writer.WriteEndObject();
        }
        writer.WriteEndArray();
        if (request.Select!= null)
        {
            writer.WritePropertyName("select");
            writer.WriteStartObject();
            writer.WritePropertyName("fields");
            writer.WriteStartArray();
            foreach (var select in request.Select.SelectQuery)
            {
                writer.WriteStartObject();
                writer.WritePropertyName("fieldPath");
                writer.WriteStringValue(fieldNameFactory(select.PropertyName));
                writer.WriteEndObject();
            }
            writer.WriteEndArray();
            writer.WriteEndObject();
        }
        if (request.Where != null)
        {
            writer.WritePropertyName("where");
            writer.WriteStartObject();
            writer.WritePropertyName("compositeFilter");
            writer.WriteStartObject();
            writer.WritePropertyName("op");
            writer.WriteStringValue("AND");
            writer.WritePropertyName("filters");
            writer.WriteStartArray();
            foreach (var filter in request.Where.FilterQuery)
            {
                switch (filter)
                {
                    case UnaryFilterQuery unaryFilter:

                        writer.WriteStartObject();
                        writer.WritePropertyName("unaryFilter");
                        writer.WriteStartObject();
                        writer.WritePropertyName("op");
                        writer.WriteStringValue(unaryFilter.Operator.ToEnumString());
                        writer.WritePropertyName("field");
                        writer.WriteStartObject();
                        writer.WritePropertyName("fieldPath");
                        writer.WriteStringValue(fieldNameFactory(filter.PropertyName));
                        writer.WriteEndObject();
                        writer.WriteEndObject();
                        writer.WriteEndObject();

                        break;
                    case FieldFilterQuery fieldFilter:

                        writer.WriteStartObject();
                        writer.WritePropertyName("fieldFilter");
                        writer.WriteStartObject();
                        writer.WritePropertyName("op");
                        writer.WriteStringValue(fieldFilter.Operator.ToEnumString());
                        writer.WritePropertyName("field");
                        writer.WriteStartObject();
                        writer.WritePropertyName("fieldPath");
                        writer.WriteStringValue(fieldNameFactory(filter.PropertyName));
                        writer.WriteEndObject();
                        writer.WritePropertyName("value");
                        ModelHelpers.BuildUtf8JsonWriterObject(config, writer, fieldFilter.Value?.GetType(), fieldFilter.Value, jsonSerializerOptions, null, null);
                        writer.WriteEndObject();
                        writer.WriteEndObject();

                        break;
                    default:
                        throw new NotImplementedException($"{filter.GetType()} Filter is not implemented.");
                }
            }
            writer.WriteEndArray();
            writer.WriteEndObject();
            writer.WriteEndObject();
        }
        if (request.OrderBy != null)
        {
            writer.WritePropertyName("orderBy");
            writer.WriteStartArray();
            foreach (var orderBy in request.OrderBy.OrderByQuery)
            {
                writer.WriteStartObject();
                writer.WritePropertyName("field");
                writer.WriteStartObject();
                writer.WritePropertyName("fieldPath");
                writer.WriteStringValue(fieldNameFactory(orderBy.PropertyName));
                writer.WriteEndObject();
                writer.WritePropertyName("direction");
                writer.WriteStringValue(orderBy.OrderDirection == OrderDirection.Ascending ? "ASCENDING" : "DESCENDING");
                writer.WriteEndObject();
            }
            writer.WriteEndArray();
        }
        if (request.StartAt != null)
        {
            writer.WritePropertyName("startAt");
            writer.WriteStartObject();
            writer.WritePropertyName("values");
            writer.WriteStartArray();
            foreach (var cursor in request.StartAt.CursorQuery)
            {
                ModelHelpers.BuildUtf8JsonWriterObject(config, writer, cursor.Value?.GetType(), cursor.Value, jsonSerializerOptions, null, null);
            }
            writer.WriteEndArray();
            writer.WritePropertyName("before");
            writer.WriteBooleanValue(request.StartAt.JustBeforeOrAfter);
            writer.WriteEndObject();
        }
        if (request.EndAt != null)
        {
            writer.WritePropertyName("endAt");
            writer.WriteStartObject();
            writer.WritePropertyName("values");
            writer.WriteStartArray();
            foreach (var cursor in request.EndAt.CursorQuery)
            {
                ModelHelpers.BuildUtf8JsonWriterObject(config, writer, cursor.Value?.GetType(), cursor.Value, jsonSerializerOptions, null, null);
            }
            writer.WriteEndArray();
            writer.WritePropertyName("before");
            writer.WriteBooleanValue(request.EndAt.JustBeforeOrAfter);
            writer.WriteEndObject();
        }
        writer.WritePropertyName("offset");
        writer.WriteNumberValue(offset);
        writer.WritePropertyName("limit");
        writer.WriteNumberValue(limit);
        writer.WriteEndObject();
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

        stream.Seek(0, SeekOrigin.Begin);
        StreamReader s = new(stream);
        var sssss = s.ReadToEnd();

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
/// Request to run a query.
/// </summary>
public abstract class BaseQueryDocumentRequest<TRequest, TResult> : FirestoreDatabaseRequest<TransactionResponse<TRequest, TResult>>
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

    /// <summary>
    /// Gets or sets the cache <see cref="Document{T}"/> documents to get and patch.
    /// </summary>
    public Document.Builder? Document { get; set; }

    /// <summary>
    /// Gets or sets the requested <see cref="References.DocumentReference"/> of the document node.
    /// </summary>
    public DocumentReference? DocumentReference { get; set; }

    /// <summary>
    /// Gets or sets the collections to query.
    /// </summary>
    public FromQuery.Builder? From { get; set; }

    /// <summary>
    /// Gets or sets the projection to return.
    /// </summary>
    public SelectQuery.Builder? Select { get; set; }

    /// <summary>
    /// Gets or sets the filter to apply.
    /// </summary>
    public FilterQuery.Builder? Where { get; set; }

    /// <summary>
    /// Gets or sets the order to apply to the query results.
    /// </summary>
    public OrderByQuery.Builder? OrderBy { get; set; }

    /// <summary>
    /// Gets or sets the starting point for the query results.
    /// </summary>
    public CursorQuery.Builder? StartAt { get; set; }

    /// <summary>
    /// Gets or sets the end point for the query results.
    /// </summary>
    public CursorQuery.Builder? EndAt { get; set; }

    /// <summary>
    /// Gets or sets the requested page size of pager <see cref="QueryDocumentResult{T}.GetAsyncEnumerator(CancellationToken)"/>. Must be >= 1 if specified.
    /// </summary>
    public int? PageSize { get; set; }

    /// <summary>
    /// Gets or sets the page to skip of pager <see cref="QueryDocumentResult{T}.GetAsyncEnumerator(CancellationToken)"/>. Must be >= 1 if specified.
    /// </summary>
    public int? SkipPage { get; set; }

    internal int ActualPageSize => PageSize ?? 20;

    internal int ActualSkipPage => SkipPage ?? 0;
}

/// <summary>
/// Request to run a query.
/// </summary>
public class QueryDocumentRequest : BaseQueryDocumentRequest<QueryDocumentRequest, QueryDocumentResult>
{
    /// <summary>
    /// Gets or sets the type of the document model.
    /// </summary>
    public Type? ModelType { get; set; }

    /// <inheritdoc cref="QueryDocumentRequest{T}"/>
    /// <returns>
    /// The <see cref="Task"/> proxy that represents the <see cref="TransactionResponse"/> with the result <see cref="QueryDocumentResult{T}"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <see cref="TransactionRequest.Config"/> or
    /// <see cref="BaseQueryDocumentRequest{QueryDocumentRequest, QueryDocumentResult}.From"/> is a null reference.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <see cref="BaseQueryDocumentRequest{QueryDocumentRequest, QueryDocumentResult}.From"/> does not contain any query.
    /// </exception>
    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    internal override async Task<TransactionResponse<QueryDocumentRequest, QueryDocumentResult>> Execute()
    {
        ArgumentNullException.ThrowIfNull(Config);
        ArgumentNullException.ThrowIfNull(From);
        if (From.FromQuery.FirstOrDefault() is not FromQuery firstFromQuery)
        {
            throw new ArgumentException($"\"{nameof(From)}\" must contain at least one parameter.");
        }
        if (PageSize != null && PageSize <= 0)
        {
            throw new ArgumentException($"\"{nameof(PageSize)}\" is provided but value is equal or less than 0.");
        }
        if (SkipPage != null && SkipPage <= 0)
        {
            throw new ArgumentException($"\"{nameof(SkipPage)}\" is provided but value is equal or less than 0.");
        }

        JsonSerializerOptions jsonSerializerOptions = ConfigureJsonSerializerOption(JsonSerializerOptions);

        var firstResponse = await ExecutePage(ActualSkipPage, Config, From, firstFromQuery, jsonSerializerOptions, default);
        if (firstResponse.IsError)
        {
            return new(this, null, firstResponse.Error);
        }

        return new(this, firstResponse.Result, null);
    }

    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    private async Task<TransactionResponse<QueryDocumentRequest, QueryDocumentResult>> ExecutePage(
        int page,
        FirebaseConfig config,
        FromQuery.Builder fromQuery,
        FromQuery firstFromQuery,
        JsonSerializerOptions jsonSerializerOptions,
        CancellationToken cancellationToken)
    {
        object? result = await QueryDocumentRequestHelpers.ExecuteQuery(
            this,
            page,
            ActualPageSize,
            config,
            fromQuery,
            firstFromQuery,
            jsonSerializerOptions,
            name => name,
            cancellationToken);

        if (result is not JsonDocument jsonDocument)
        {
            return new(this, null, result as Exception);
        }

        List<DocumentTimestamp> foundDocuments = new();
        int? skippedResults = null;
        DateTimeOffset? skippedReadTime = null;

        foreach (var doc in jsonDocument.RootElement.EnumerateArray())
        {
            if (doc.TryGetProperty("readTime", out JsonElement readTimeProperty) &&
                readTimeProperty.GetDateTimeOffset() is DateTimeOffset readTime)
            {
                DocumentReference? documentReference = null;
                Document? document = null;
                object? model = null;
                if (doc.TryGetProperty("document", out JsonElement foundPropertyDocument))
                {
                    if (foundPropertyDocument.TryGetProperty("name", out JsonElement foundNameProperty) &&
                        DocumentReference.Parse(foundNameProperty, jsonSerializerOptions) is DocumentReference docRef)
                    {
                        documentReference = docRef;

                        if (Document != null &&
                            Document.Documents.FirstOrDefault(i => i.Reference.Equals(docRef)) is Document foundDocument)
                        {
                            document = foundDocument;
                            model = foundDocument.GetModel();
                        }
                    }

                    if (Models.Document.Parse(documentReference, ModelType ?? model?.GetType(), model, document, foundPropertyDocument.EnumerateObject(), jsonSerializerOptions) is Document found)
                    {
                        foundDocuments.Add(new DocumentTimestamp(found, readTime));
                    }
                }
                else if (
                    doc.TryGetProperty("skippedResults", out JsonElement skippedResultsProperty) &&
                    skippedResultsProperty.TryGetInt32(out int parsedSkippedResults))
                {
                    skippedResults = parsedSkippedResults;
                    skippedReadTime = readTime;
                }
            }
            else if (
                Transaction != null &&
                doc.TryGetProperty("transaction", out JsonElement transactionElement) &&
                transactionElement.GetString() is string transactionToken)
            {
                Transaction.Transaction.Token = transactionToken;
            }
        }

        TransactionResponse<QueryDocumentRequest, QueryDocumentResult> response = null!;
        response = new(
            this,
            new(foundDocuments,
                skippedResults,
                skippedReadTime,
                page,
                ActualPageSize,
                () => response,
                (pageNum, ct) =>
                {
                    return ExecutePage(pageNum, config, fromQuery, firstFromQuery, jsonSerializerOptions, ct);
                }),
            null);
        return response;
    }
}

/// <summary>
/// The result of the <see cref="QueryDocumentRequest"/> request.
/// </summary>
public class QueryDocumentResult : IAsyncEnumerable<TransactionResponse<QueryDocumentRequest, QueryDocumentResult>>
{
    /// <summary>
    /// Gets the found document.
    /// </summary>
    public IReadOnlyList<DocumentTimestamp> Documents { get; internal set; }

    /// <summary>
    /// Gets the number of results that have been skipped due to an offset between the last response and the current response.
    /// </summary>
    public int? SkippedResults { get; internal set; }

    /// <summary>
    /// Gets the time at which the skipped document was read.
    /// </summary>
    public DateTimeOffset? SkippedReadTime { get; internal set; }

    /// <summary>
    /// Gets the page number of the current page.
    /// </summary>
    public int CurrentPage { get; internal set; }

    private readonly int pageSize;
    private readonly Func<TransactionResponse<QueryDocumentRequest, QueryDocumentResult>> firstResponseFactory;
    private readonly Func<int, CancellationToken, Task<TransactionResponse<QueryDocumentRequest, QueryDocumentResult>>> pager;

    internal QueryDocumentResult(
        IReadOnlyList<DocumentTimestamp> documents,
        int? skippedResults,
        DateTimeOffset? skippedReadTime,
        int currentPage,
        int pageSize,
        Func<TransactionResponse<QueryDocumentRequest, QueryDocumentResult>> firstResponseFactory,
        Func<int, CancellationToken, Task<TransactionResponse<QueryDocumentRequest, QueryDocumentResult>>> pager)
    {
        Documents = documents;
        SkippedResults = skippedResults;
        SkippedReadTime = skippedReadTime;
        CurrentPage = currentPage;
        this.pageSize = pageSize;
        this.firstResponseFactory = firstResponseFactory;
        this.pager = pager;
    }

    /// <summary>
    /// Request to go to get page of the result query.
    /// </summary>
    /// <param name="page">
    /// The page number to go to.
    /// </param>
    /// <param name="cancellationToken">
    /// The <see cref="CancellationToken"/> of the request.
    /// </param>
    /// <returns>
    /// The <see cref="Task"/> proxy that represents the <see cref="TransactionResponse"/> with the result <see cref="QueryDocumentResult"/>.
    /// </returns>
    public Task<TransactionResponse<QueryDocumentRequest, QueryDocumentResult>> GetPage(int page, CancellationToken cancellationToken = default)
    {
        return pager.Invoke(page, cancellationToken);
    }

    /// <inheritdoc/>
    public IAsyncEnumerator<TransactionResponse<QueryDocumentRequest, QueryDocumentResult>> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
        return new AsyncEnumerator(pageSize, firstResponseFactory(), CancellationTokenSource.CreateLinkedTokenSource(cancellationToken));
    }

    internal class AsyncEnumerator : IAsyncEnumerator<TransactionResponse<QueryDocumentRequest, QueryDocumentResult>>
    {
        public TransactionResponse<QueryDocumentRequest, QueryDocumentResult> Current { get; private set; } = default!;

        private QueryDocumentResult lastSuccessResult;

        private readonly int pageSize;
        private readonly TransactionResponse<QueryDocumentRequest, QueryDocumentResult> firstResponse;
        private readonly CancellationTokenSource cancellationTokenSource;

        public AsyncEnumerator(int pageSize, TransactionResponse<QueryDocumentRequest, QueryDocumentResult> firstResponse, CancellationTokenSource cancellationTokenSource)
        {
            firstResponse.ThrowIfError();
            this.pageSize = pageSize;
            this.firstResponse = firstResponse;
            lastSuccessResult = firstResponse.Result;
            this.cancellationTokenSource = cancellationTokenSource;
        }

        public ValueTask DisposeAsync()
        {
            cancellationTokenSource.Cancel();
            return new ValueTask();
        }

        public async ValueTask<bool> MoveNextAsync()
        {
            if (Current == null)
            {
                Current = firstResponse;
                return true;
            }
            else
            {
                if (lastSuccessResult.Documents.Count < pageSize)
                {
                    return false;
                }
                else
                {
                    Current = await lastSuccessResult.pager.Invoke(lastSuccessResult.CurrentPage + 1, cancellationTokenSource.Token);
                    if (Current.IsSuccess)
                    {
                        lastSuccessResult = Current.Result;
                        return lastSuccessResult.Documents.Count != 0;
                    }
                    return true;
                }
            }
        }
    }
}

/// <summary>
/// Request to run a query.
/// </summary>
public class QueryDocumentRequest<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T> : BaseQueryDocumentRequest<QueryDocumentRequest<T>, QueryDocumentResult<T>>
    where T : class
{
    /// <inheritdoc cref="QueryDocumentRequest{T}"/>
    /// <returns>
    /// The <see cref="Task"/> proxy that represents the <see cref="TransactionResponse"/> with the result <see cref="QueryDocumentResult{T}"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <see cref="TransactionRequest.Config"/> or
    /// <see cref="BaseQueryDocumentRequest{QueryDocumentRequest, QueryDocumentResult}.From"/> is a null reference.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <see cref="BaseQueryDocumentRequest{QueryDocumentRequest, QueryDocumentResult}.From"/> does not contain any query.
    /// </exception>
    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    internal override async Task<TransactionResponse<QueryDocumentRequest<T>, QueryDocumentResult<T>>> Execute()
    {
        ArgumentNullException.ThrowIfNull(Config);
        ArgumentNullException.ThrowIfNull(From);
        if (From.FromQuery.FirstOrDefault() is not FromQuery firstFromQuery)
        {
            throw new ArgumentException($"\"{nameof(From)}\" must contain at least one parameter.");
        }
        if (PageSize != null && PageSize <= 0)
        {
            throw new ArgumentException($"\"{nameof(PageSize)}\" is provided but value is equal or less than 0.");
        }
        if (SkipPage != null && SkipPage <= 0)
        {
            throw new ArgumentException($"\"{nameof(SkipPage)}\" is provided but value is equal or less than 0.");
        }

        JsonSerializerOptions jsonSerializerOptions = ConfigureJsonSerializerOption(JsonSerializerOptions);

        Type objType = typeof(T);

        PropertyInfo[] propertyInfos = objType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        FieldInfo[] fieldInfos = objType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        bool includeOnlyWithAttribute = objType.GetCustomAttribute(typeof(FirebaseValueOnlyAttribute)) != null;

        var firstResponse = await ExecutePage(ActualSkipPage, Config, From, firstFromQuery, objType, propertyInfos, fieldInfos, includeOnlyWithAttribute, jsonSerializerOptions, default);
        if (firstResponse.IsError)
        {
            return new(this, null, firstResponse.Error);
        }

        return new(this, firstResponse.Result, null);
    }

    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    private async Task<TransactionResponse<QueryDocumentRequest<T>, QueryDocumentResult<T>>> ExecutePage(
        int page,
        FirebaseConfig config,
        FromQuery.Builder fromQuery,
        FromQuery firstFromQuery,
        Type objType,
        PropertyInfo[] propertyInfos,
        FieldInfo[] fieldInfos,
        bool includeOnlyWithAttribute,
        JsonSerializerOptions jsonSerializerOptions,
        CancellationToken cancellationToken)
    {
        object? result = await QueryDocumentRequestHelpers.ExecuteQuery(
            this,
            page,
            ActualPageSize,
            config,
            fromQuery,
            firstFromQuery,
            jsonSerializerOptions,
            name =>
            {
                if (name == DocumentFieldHelpers.DocumentName)
                {
                    return name;
                }
                else
                {
                    var documentField = ClassMemberHelpers.GetDocumentField(propertyInfos, fieldInfos, includeOnlyWithAttribute, name, jsonSerializerOptions);

                    if (documentField == null)
                    {
                        throw new ArgumentException($"\"{name}\" does not exist in the model \"{objType.Name}\".");
                    }

                    return documentField.DocumentFieldName;
                }
            },
            cancellationToken);

        if (result is not JsonDocument jsonDocument)
        {
            return new(this, null, result as Exception);
        }

        List<DocumentTimestamp<T>> foundDocuments = new();
        int? skippedResults = null;
        DateTimeOffset? skippedReadTime = null;

        foreach (var doc in jsonDocument.RootElement.EnumerateArray())
        {
            if (doc.TryGetProperty("readTime", out JsonElement readTimeProperty) &&
                readTimeProperty.GetDateTimeOffset() is DateTimeOffset readTime)
            {
                DocumentReference? documentReference = null;
                Document<T>? document = null;
                T? model = null;
                if (doc.TryGetProperty("document", out JsonElement foundPropertyDocument))
                {
                    if (foundPropertyDocument.TryGetProperty("name", out JsonElement foundNameProperty) &&
                        DocumentReference.Parse(foundNameProperty, jsonSerializerOptions) is DocumentReference docRef)
                    {
                        documentReference = docRef;

                        if (Document != null &&
                            Document.Documents.FirstOrDefault(i => i.Reference.Equals(docRef)) is Document<T> foundDocument)
                        {
                            document = foundDocument;
                            model = foundDocument.Model;
                        }
                    }

                    if (Document<T>.Parse(documentReference, model, document, foundPropertyDocument.EnumerateObject(), jsonSerializerOptions) is Document<T> found)
                    {
                        foundDocuments.Add(new DocumentTimestamp<T>(found, readTime));
                    }
                }
                else if (
                    doc.TryGetProperty("skippedResults", out JsonElement skippedResultsProperty) &&
                    skippedResultsProperty.TryGetInt32(out int parsedSkippedResults))
                {
                    skippedResults = parsedSkippedResults;
                    skippedReadTime = readTime;
                }
            }
            else if (
                Transaction != null &&
                doc.TryGetProperty("transaction", out JsonElement transactionElement) &&
                transactionElement.GetString() is string transactionToken)
            {
                Transaction.Transaction.Token = transactionToken;
            }
        }

        TransactionResponse<QueryDocumentRequest<T>, QueryDocumentResult<T>> response = null!;
        response = new(
            this,
            new(foundDocuments,
                skippedResults,
                skippedReadTime,
                page,
                ActualPageSize,
                () => response,
                (pageNum, ct) =>
                {
                    return ExecutePage(pageNum, config, fromQuery, firstFromQuery, objType, propertyInfos, fieldInfos, includeOnlyWithAttribute, jsonSerializerOptions, ct);
                }),
            null);
        return response;
    }
}

/// <summary>
/// The result of the <see cref="QueryDocumentRequest{T}"/> request.
/// </summary>
/// <typeparam name="T">
/// The type of the model of the document.
/// </typeparam>
public class QueryDocumentResult<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T> : IAsyncEnumerable<TransactionResponse<QueryDocumentRequest<T>, QueryDocumentResult<T>>>
    where T : class
{
    /// <summary>
    /// Gets the found document.
    /// </summary>
    public IReadOnlyList<DocumentTimestamp<T>> Documents { get; internal set; }

    /// <summary>
    /// Gets the number of results that have been skipped due to an offset between the last response and the current response.
    /// </summary>
    public int? SkippedResults { get; internal set; }

    /// <summary>
    /// Gets the time at which the skipped document was read.
    /// </summary>
    public DateTimeOffset? SkippedReadTime { get; internal set; }

    /// <summary>
    /// Gets the page number of the current page.
    /// </summary>
    public int CurrentPage { get; internal set; }

    private readonly int pageSize;
    private readonly Func<TransactionResponse<QueryDocumentRequest<T>, QueryDocumentResult<T>>> firstResponseFactory;
    private readonly Func<int, CancellationToken, Task<TransactionResponse<QueryDocumentRequest<T>, QueryDocumentResult<T>>>> pager;

    internal QueryDocumentResult(
        IReadOnlyList<DocumentTimestamp<T>> documents,
        int? skippedResults,
        DateTimeOffset? skippedReadTime,
        int currentPage,
        int pageSize,
        Func<TransactionResponse<QueryDocumentRequest<T>, QueryDocumentResult<T>>> firstResponseFactory,
        Func<int, CancellationToken, Task<TransactionResponse<QueryDocumentRequest<T>, QueryDocumentResult<T>>>> pager)
    {
        Documents = documents;
        SkippedResults = skippedResults;
        SkippedReadTime = skippedReadTime;
        CurrentPage = currentPage;
        this.pageSize = pageSize;
        this.firstResponseFactory = firstResponseFactory;
        this.pager = pager;
    }

    /// <summary>
    /// Request to go to get page of the result query.
    /// </summary>
    /// <param name="page">
    /// The page number to go to.
    /// </param>
    /// <param name="cancellationToken">
    /// The <see cref="CancellationToken"/> of the request.
    /// </param>
    /// <returns>
    /// The <see cref="Task"/> proxy that represents the <see cref="TransactionResponse"/> with the result <see cref="QueryDocumentResult{T}"/>.
    /// </returns>
    public Task<TransactionResponse<QueryDocumentRequest<T>, QueryDocumentResult<T>>> GetPage(int page, CancellationToken cancellationToken = default)
    {
        return pager.Invoke(page, cancellationToken);
    }

    /// <inheritdoc/>
    public IAsyncEnumerator<TransactionResponse<QueryDocumentRequest<T>, QueryDocumentResult<T>>> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
        return new AsyncEnumerator(pageSize, firstResponseFactory(), CancellationTokenSource.CreateLinkedTokenSource(cancellationToken));
    }

    internal class AsyncEnumerator : IAsyncEnumerator<TransactionResponse<QueryDocumentRequest<T>, QueryDocumentResult<T>>>
    {
        public TransactionResponse<QueryDocumentRequest<T>, QueryDocumentResult<T>> Current { get; private set; } = default!;

        private QueryDocumentResult<T> lastSuccessResult;

        private readonly int pageSize;
        private readonly TransactionResponse<QueryDocumentRequest<T>, QueryDocumentResult<T>> firstResponse;
        private readonly CancellationTokenSource cancellationTokenSource;

        public AsyncEnumerator(int pageSize, TransactionResponse<QueryDocumentRequest<T>, QueryDocumentResult<T>> firstResponse, CancellationTokenSource cancellationTokenSource)
        {
            firstResponse.ThrowIfError();
            this.pageSize = pageSize;
            this.firstResponse = firstResponse;
            lastSuccessResult = firstResponse.Result;
            this.cancellationTokenSource = cancellationTokenSource;
        }

        public ValueTask DisposeAsync()
        {
            cancellationTokenSource.Cancel();
            return new ValueTask();
        }

        public async ValueTask<bool> MoveNextAsync()
        {
            if (Current == null)
            {
                Current = firstResponse;
                return true;
            }
            else
            {
                if (lastSuccessResult.Documents.Count < pageSize)
                {
                    return false;
                }
                else
                {
                    Current = await lastSuccessResult.pager.Invoke(lastSuccessResult.CurrentPage + 1, cancellationTokenSource.Token);
                    if (Current.IsSuccess)
                    {
                        lastSuccessResult = Current.Result;
                        return lastSuccessResult.Documents.Count != 0;
                    }
                    return true;
                }
            }
        }
    }
}
