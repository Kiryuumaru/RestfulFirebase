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

/// <summary>
/// Request to run a query.
/// </summary>
public class QueryDocumentRequest<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T> : FirestoreDatabaseRequest<TransactionResponse<QueryDocumentRequest<T>, QueryDocumentResult<T>>>
    where T : class
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
    public Document<T>.Builder? Document { get; set; }

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
    /// Gets or sets the requested page size of pager <see cref="QueryDocumentResult{T}.GetAsyncEnumerator(CancellationToken)"/>. Must be >= 1 if specified.
    /// </summary>
    public int? PageSize { get; set; }

    /// <summary>
    /// Gets or sets the page to skip of pager <see cref="QueryDocumentResult{T}.GetAsyncEnumerator(CancellationToken)"/>. Must be >= 1 if specified.
    /// </summary>
    public int? SkipPage { get; set; }

    private int ActualPageSize => PageSize ?? 20;

    private int ActualSkipPage => SkipPage ?? 0;

    /// <inheritdoc cref="QueryDocumentRequest{T}"/>
    /// <returns>
    /// The <see cref="Task"/> proxy that represents the <see cref="TransactionResponse"/> with the result <see cref="QueryDocumentResult{T}"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <see cref="TransactionRequest.Config"/> or
    /// <see cref="From"/> is a null reference.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <see cref="From"/> does not contain any query.
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
        CancellationToken linkedCancellationToken = CancellationTokenSource.CreateLinkedTokenSource(CancellationToken, cancellationToken).Token;

        int limit = ActualPageSize;
        int offset = page * limit;

        string url;
        if (DocumentReference != null)
        {
            if (fromQuery.FromQuery.Any(i => i.CollectionReference.Parent != null && i.CollectionReference.Parent != DocumentReference))
            {
                throw new ArgumentException($"\"{nameof(DocumentReference)}\" is provided but one or more \"{nameof(From)}\" has different parent document.");
            }

            url = DocumentReference.BuildUrl(config.ProjectId, ":runQuery");
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
                    throw new ArgumentException($"\"{nameof(From)}\" has different parent document.");
                }

                url =
                    $"{Api.FirestoreDatabase.FirestoreDatabaseV1Endpoint}/" +
                    $"{string.Format(Api.FirestoreDatabase.FirestoreDatabaseDocumentsEndpoint, config.ProjectId, ":runQuery")}";
            }
        }

        using MemoryStream stream = new();
        Utf8JsonWriter writer = new(stream);

        writer.WriteStartObject();
        ArgumentNullException.ThrowIfNull(Config);
        ArgumentNullException.ThrowIfNull(From);

        writer.WritePropertyName("structuredQuery");
        writer.WriteStartObject();
        writer.WritePropertyName("from");
        writer.WriteStartArray();
        foreach (var from in From.FromQuery)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("collectionId");
            writer.WriteStringValue(from.CollectionReference.Id);
            writer.WritePropertyName("allDescendants");
            writer.WriteBooleanValue(from.AllDescendants);
            writer.WriteEndObject();
        }
        writer.WriteEndArray();
        if (Select != null)
        {
            writer.WritePropertyName("select");
            writer.WriteStartObject();
            writer.WritePropertyName("fields");
            writer.WriteStartArray();
            foreach (var select in Select.SelectQuery)
            {
                string fieldName;
                if (Select.IsDocumentNameOnly)
                {
                    fieldName = select.PropertyName;
                }
                else
                {
                    var documentField = ClassMemberHelpers.GetDocumentField(propertyInfos, fieldInfos, includeOnlyWithAttribute, select.PropertyName, jsonSerializerOptions);

                    if (documentField == null)
                    {
                        throw new ArgumentException($"\"{select.PropertyName}\" does not exist in the model \"{objType.Name}\".");
                    }

                    fieldName = documentField.DocumentFieldName;
                }

                writer.WriteStartObject();
                writer.WritePropertyName("fieldPath");
                writer.WriteStringValue(fieldName);
                writer.WriteEndObject();
            }
            writer.WriteEndArray();
            writer.WriteEndObject();
        }
        if (Where != null)
        {
            writer.WritePropertyName("where");
            writer.WriteStartObject();
            writer.WritePropertyName("compositeFilter");
            writer.WriteStartObject();
            writer.WritePropertyName("op");
            writer.WriteStringValue("AND");
            writer.WritePropertyName("filters");
            writer.WriteStartArray();
            foreach (var filter in Where.FilterQuery)
            {
                string fieldName;
                if (filter.PropertyName == DocumentFieldHelpers.DocumentName)
                {
                    fieldName = filter.PropertyName;
                }
                else
                {
                    var documentField = ClassMemberHelpers.GetDocumentField(propertyInfos, fieldInfos, includeOnlyWithAttribute, filter.PropertyName, jsonSerializerOptions);

                    if (documentField == null)
                    {
                        throw new ArgumentException($"\"{filter.PropertyName}\" does not exist in the model \"{objType.Name}\".");
                    }

                    fieldName = documentField.DocumentFieldName;
                }

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
                        writer.WriteStringValue(fieldName);
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
                        writer.WriteStringValue(fieldName);
                        writer.WriteEndObject();
                        writer.WritePropertyName("value");
                        ModelHelpers.BuildUtf8JsonWriterObject(Config, writer, fieldFilter.Value?.GetType(), fieldFilter.Value, jsonSerializerOptions, null, null);
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
        if (OrderBy != null)
        {
            writer.WritePropertyName("orderBy");
            writer.WriteStartArray();
            foreach (var orderBy in OrderBy.OrderByQuery)
            {
                string fieldName;
                if (orderBy.PropertyName == DocumentFieldHelpers.DocumentName)
                {
                    fieldName = orderBy.PropertyName;
                }
                else
                {
                    var documentField = ClassMemberHelpers.GetDocumentField(propertyInfos, fieldInfos, includeOnlyWithAttribute, orderBy.PropertyName, jsonSerializerOptions);

                    if (documentField == null)
                    {
                        throw new ArgumentException($"\"{orderBy.PropertyName}\" does not exist in the model \"{objType.Name}\".");
                    }

                    fieldName = documentField.DocumentFieldName;
                }

                writer.WriteStartObject();
                writer.WritePropertyName("field");
                writer.WriteStartObject();
                writer.WritePropertyName("fieldPath");
                writer.WriteStringValue(fieldName);
                writer.WriteEndObject();
                writer.WritePropertyName("direction");
                writer.WriteStringValue(orderBy.OrderDirection == OrderDirection.Ascending ? "ASCENDING" : "DESCENDING");
                writer.WriteEndObject();
            }
            writer.WriteEndArray();
        }
        writer.WritePropertyName("offset");
        writer.WriteNumberValue(offset);
        writer.WritePropertyName("limit");
        writer.WriteNumberValue(limit);
        writer.WriteEndObject();
        if (Transaction != null)
        {
            if (Transaction.Transaction.Token == null)
            {
                writer.WritePropertyName("newTransaction");
                if (Transaction.Transaction is ReadOnlyTransaction readOnlyTransaction)
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
                else if (Transaction.Transaction is ReadWriteTransaction readWriteTransaction)
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
                writer.WriteStringValue(Transaction.Transaction.Token);
            }
        }
        writer.WriteEndObject();

        await writer.FlushAsync(linkedCancellationToken);

        var (executeResult, executeException) = await ExecuteWithContent(stream, HttpMethod.Post, url);
        if (executeResult == null)
        {
            return new(this, null, executeException);
        }

#if NET6_0_OR_GREATER
        using Stream contentStream = await executeResult.Content.ReadAsStreamAsync(linkedCancellationToken);
#else
        using Stream contentStream = await executeResult.Content.ReadAsStreamAsync();
#endif
        JsonDocument jsonDocument = await JsonDocument.ParseAsync(contentStream, cancellationToken: linkedCancellationToken);

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
