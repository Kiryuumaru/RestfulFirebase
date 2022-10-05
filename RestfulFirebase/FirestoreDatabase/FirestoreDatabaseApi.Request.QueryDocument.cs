using System;
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
using System.Xml.Linq;
using RestfulFirebase.Common.Abstractions;
using RestfulFirebase.Common.Http;
using RestfulFirebase.Common.Internals;

namespace RestfulFirebase.FirestoreDatabase;

public partial class FirestoreDatabaseApi
{
    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    internal async Task<(JsonDocument?, HttpResponse)> ExecuteQueryDocument(
        int page,
        int pageSize,
        int offset,
        DocumentReference? documentReference,
        FromQuery.Builder fromQuery,
        FromQuery firstFromQuery,
        SelectQuery.Builder? selectQuery,
        FilterQuery.Builder? filterQuery,
        List<(string fieldPath, Direction direction)>? orderBy,
        List<object>? startCursors,
        List<object>? endCursors,
        bool? isStartAfter,
        bool? isEndBefore,
        Func<string[], string> fieldPathFactory,
        Transaction? transaction,
        IAuthorization? authorization,
        JsonSerializerOptions jsonSerializerOptions,
        CancellationToken cancellationToken)
    {
        string url;
        if (documentReference != null)
        {
            if (fromQuery.FromQuery.Any(i => i.CollectionReference.Parent != null && i.CollectionReference.Parent != documentReference))
            {
                throw new ArgumentException($"\"{nameof(documentReference)}\" is provided but one or more \"{nameof(fromQuery)}\" has different parent document.");
            }

            url = documentReference.BuildUrl(App.Config.ProjectId, ":runQuery");
        }
        else
        {
            if (fromQuery.FromQuery.Count == 1 &&
                firstFromQuery.CollectionReference.Parent != null)
            {
                url = firstFromQuery.CollectionReference.Parent.BuildUrl(App.Config.ProjectId, ":runQuery");
            }
            else
            {
                if (fromQuery.FromQuery.Any(i => i.CollectionReference.Parent != firstFromQuery.CollectionReference.Parent))
                {
                    throw new ArgumentException($"\"{nameof(fromQuery)}\" has different parent document.");
                }

                url =
                    $"{FirestoreDatabaseV1Endpoint}/" +
                    $"{string.Format(FirestoreDatabaseDocumentsEndpoint, App.Config.ProjectId, ":runQuery")}";
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
        if (selectQuery != null)
        {
            writer.WritePropertyName("select");
            writer.WriteStartObject();
            writer.WritePropertyName("fields");
            writer.WriteStartArray();
            foreach (var select in selectQuery.SelectQuery)
            {
                writer.WriteStartObject();
                writer.WritePropertyName("fieldPath");
                writer.WriteStringValue(fieldPathFactory(select.NamePath));
                writer.WriteEndObject();
            }
            writer.WriteEndArray();
            writer.WriteEndObject();
        }
        if (filterQuery != null)
        {
            writer.WritePropertyName("where");
            writer.WriteStartObject();
            writer.WritePropertyName("compositeFilter");
            writer.WriteStartObject();
            writer.WritePropertyName("op");
            writer.WriteStringValue("AND");
            writer.WritePropertyName("filters");
            writer.WriteStartArray();
            foreach (var filter in filterQuery.FilterQuery)
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
                        writer.WriteStringValue(fieldPathFactory(filter.NamePath));
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
                        writer.WriteStringValue(fieldPathFactory(filter.NamePath));
                        writer.WriteEndObject();
                        writer.WritePropertyName("value");
                        ModelBuilderHelpers.BuildUtf8JsonWriterObject(App.Config, writer, fieldFilter.Value?.GetType(), fieldFilter.Value, jsonSerializerOptions, null, null);
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

        if (orderBy != null)
        {
            writer.WritePropertyName("orderBy");
            writer.WriteStartArray();
            foreach (var (fieldPath, direction) in orderBy)
            {
                writer.WriteStartObject();
                writer.WritePropertyName("field");
                writer.WriteStartObject();
                writer.WritePropertyName("fieldPath");
                writer.WriteStringValue(fieldPath);
                writer.WriteEndObject();
                writer.WritePropertyName("direction");
                writer.WriteStringValue(direction == Direction.Ascending ? "ASCENDING" : "DESCENDING");
                writer.WriteEndObject();
            }
            writer.WriteEndArray();
        }

        if (startCursors != null && isStartAfter.HasValue)
        {
            writer.WritePropertyName("startAt");
            writer.WriteStartObject();
            writer.WritePropertyName("values");
            writer.WriteStartArray();
            foreach (var cursor in startCursors)
            {
                ModelBuilderHelpers.BuildUtf8JsonWriterObject(App.Config, writer, cursor?.GetType(), cursor, jsonSerializerOptions, null, null);
            }
            writer.WriteEndArray();
            writer.WritePropertyName("before");
            writer.WriteBooleanValue(isStartAfter.Value);
            writer.WriteEndObject();
        }

        if (endCursors != null && isEndBefore.HasValue)
        {
            writer.WritePropertyName("endAt");
            writer.WriteStartObject();
            writer.WritePropertyName("values");
            writer.WriteStartArray();
            foreach (var cursor in endCursors)
            {
                ModelBuilderHelpers.BuildUtf8JsonWriterObject(App.Config, writer, cursor?.GetType(), cursor, jsonSerializerOptions, null, null);
            }
            writer.WriteEndArray();
            writer.WritePropertyName("before");
            writer.WriteBooleanValue(!isEndBefore.Value);
            writer.WriteEndObject();
        }

        writer.WritePropertyName("offset");
        writer.WriteNumberValue(offset);
        writer.WritePropertyName("limit");
        writer.WriteNumberValue(pageSize);
        writer.WriteEndObject();
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

    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    public async Task<HttpResponse<QueryDocumentResult>> QueryDocument(
        FromQuery.Builder fromQuery,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Type? objType = default,
        int pageSize = 20,
        int skipPage = 0,
        DocumentReference? documentReference = default,
        SelectQuery.Builder? selectQuery = default,
        FilterQuery.Builder? filterQuery = default,
        OrderQuery.Builder? orderQuery = default,
        IEnumerable<Document>? cacheDocuments = default,
        Transaction? transaction = default,
        IAuthorization? authorization = default,
        JsonSerializerOptions? jsonSerializerOptions = default,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(fromQuery);
        
        if (fromQuery.FromQuery.FirstOrDefault() is not FromQuery firstFromQuery)
        {
            throw new ArgumentException($"\"{nameof(fromQuery)}\" must contain at least one argument.");
        }
        if (pageSize <= 0)
        {
            throw new ArgumentException($"\"{nameof(pageSize)}\" is equal or less than 0.");
        }
        if (skipPage <= 0)
        {
            throw new ArgumentException($"\"{nameof(skipPage)}\" is equal or less than 0.");
        }

        JsonSerializerOptions configuredJsonSerializerOptions = ConfigureJsonSerializerOption(jsonSerializerOptions);

        return await QueryDocumentPage(
            new(),
            0,
            objType,
            fromQuery,
            firstFromQuery,
            pageSize,
            skipPage,
            documentReference,
            selectQuery,
            filterQuery,
            orderQuery,

            transaction,
            cacheDocuments,
            authorization,
            configuredJsonSerializerOptions,
            cancellationToken);
    }

    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    public async Task<HttpResponse<QueryDocumentResult<T>>> QueryDocument<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>(
        FromQuery.Builder fromQuery,
        int pageSize = 20,
        int skipPage = 0,
        DocumentReference? documentReference = default,
        SelectQuery.Builder? selectQuery = default,
        FilterQuery.Builder? filterQuery = default,
        OrderQuery.Builder? orderQuery = default,
        IEnumerable<Document>? cacheDocuments = default,
        Transaction? transaction = default,
        IAuthorization? authorization = default,
        JsonSerializerOptions? jsonSerializerOptions = default,
        CancellationToken cancellationToken = default)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(fromQuery);

        if (fromQuery.FromQuery.FirstOrDefault() is not FromQuery firstFromQuery)
        {
            throw new ArgumentException($"\"{nameof(fromQuery)}\" must contain at least one argument.");
        }
        if (pageSize <= 0)
        {
            throw new ArgumentException($"\"{nameof(pageSize)}\" is equal or less than 0.");
        }
        if (skipPage <= 0)
        {
            throw new ArgumentException($"\"{nameof(skipPage)}\" is equal or less than 0.");
        }

        JsonSerializerOptions configuredJsonSerializerOptions = ConfigureJsonSerializerOption(jsonSerializerOptions);

        return await QueryDocumentPage<T>(
            new(),
            0,
            fromQuery,
            firstFromQuery,
            pageSize,
            skipPage,
            documentReference,
            selectQuery,
            filterQuery,
            orderQuery,

            transaction,
            cacheDocuments,
            authorization,
            configuredJsonSerializerOptions,
            cancellationToken);
    }

    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    private async Task<HttpResponse<QueryDocumentResult>> QueryDocumentPage(
        HttpResponse<QueryDocumentResult> response,
        int page,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Type? objType,
        FromQuery.Builder fromQuery,
        FromQuery firstFromQuery,
        int pageSize,
        int offset,
        DocumentReference? documentReference,
        SelectQuery.Builder? selectQuery,
        FilterQuery.Builder? filterQuery,
        OrderQuery.Builder? orderQuery,
        Transaction? transaction,
        IEnumerable<Document>? cacheDocuments,
        IAuthorization? authorization,
        JsonSerializerOptions jsonSerializerOptions,
        CancellationToken cancellationToken)
    {
        List<(string fieldPath, Direction direction)>? orderBys = null;
        List<object>? startCursors = null;
        List<object>? endCursors = null;
        bool? isStartAfter = null;
        bool? isEndBefore = null;

        if (orderQuery != null)
        {
            orderBys = new();
            startCursors = new();
            endCursors = new();
            isStartAfter = orderQuery.IsStartAfter;
            isEndBefore = orderQuery.IsEndBefore;

            bool hasDocumentName = false;
            foreach (var query in orderQuery.OrderQuery)
            {
                var documentFieldPath = DocumentFieldHelpers.GetDocumentFieldPath(query.ModelType, query.NamePath, jsonSerializerOptions);
                var lastDocumentFieldPath = documentFieldPath.LastOrDefault()!;

                if (documentFieldPath[0].DocumentFieldName == DocumentFieldHelpers.DocumentName)
                {
                    if (hasDocumentName)
                    {
                        throw new ArgumentException($"\"{nameof(orderQuery)}\" must only contain a single document name.");
                    }
                    hasDocumentName = true;
                }
                else if (hasDocumentName)
                {
                    throw new ArgumentException($"\"{nameof(orderQuery)}\" with document name must be place on the end of the query.");
                }

                orderBys.Add((string.Join(".", documentFieldPath.Select(i => i.DocumentFieldName)), query.Direction));
                startCursors.Add(query.StartPosition ?? DocumentFieldHelpers.GetLimitValue(lastDocumentFieldPath.Type, query.Direction == Direction.Ascending));
                endCursors.Add(query.EndPosition ?? DocumentFieldHelpers.GetLimitValue(lastDocumentFieldPath.Type, query.Direction == Direction.Ascending));
            }
        }

        var (jsonDocument, queryDocumentResponse) = await ExecuteQueryDocument(
            page,
            pageSize,
            offset,
            documentReference,
            fromQuery,
            firstFromQuery,
            selectQuery,
            filterQuery,
            orderBys,
            startCursors,
            endCursors,
            isStartAfter,
            isEndBefore,
            namePath =>
            {
                if (objType == null)
                {
                    return string.Join(".", namePath);
                }
                else
                {
                    bool hasDocumentName = false;
                    var documentFieldPath = DocumentFieldHelpers.GetDocumentFieldPath(objType, namePath, jsonSerializerOptions);

                    if (documentFieldPath[0].DocumentFieldName == DocumentFieldHelpers.DocumentName)
                    {
                        if (hasDocumentName)
                        {
                            throw new ArgumentException($"\"{nameof(orderQuery)}\" must only contain a single document name.");
                        }
                        hasDocumentName = true;
                    }
                    else if (hasDocumentName)
                    {
                        throw new ArgumentException($"\"{nameof(orderQuery)}\" with document name must be place on the end of the query.");
                    }

                    return string.Join(".", documentFieldPath.Select(i => i.DocumentFieldName));
                }
            },
            transaction,
            authorization,
            jsonSerializerOptions,
            cancellationToken);
        response.Concat(queryDocumentResponse);
        if (queryDocumentResponse.IsError || jsonDocument == null)
        {
            return response;
        }

        List<DocumentTimestamp> foundDocuments = new();
        int? skippedResults = null;
        DateTimeOffset? skippedReadTime = null;

        foreach (var doc in jsonDocument.RootElement.EnumerateArray())
        {
            if (doc.TryGetProperty("readTime", out JsonElement readTimeProperty) &&
                readTimeProperty.GetDateTimeOffset() is DateTimeOffset readTime)
            {
                DocumentReference? parsedDocumentReference = null;
                Document? parsedDocument = null;
                object? parsedModel = null;
                if (doc.TryGetProperty("document", out JsonElement foundPropertyDocument))
                {
                    if (foundPropertyDocument.TryGetProperty("name", out JsonElement foundNameProperty) &&
                        DocumentReference.Parse(App, foundNameProperty, jsonSerializerOptions) is DocumentReference docRef)
                    {
                        parsedDocumentReference = docRef;

                        if (cacheDocuments != null &&
                            cacheDocuments.FirstOrDefault(i => i.Reference.Equals(docRef)) is Document foundDocument)
                        {
                            parsedDocument = foundDocument;
                            parsedModel = foundDocument.GetModel();
                        }
                    }

                    if (Document.Parse(App, documentReference, parsedModel?.GetType(), parsedModel, parsedDocument, foundPropertyDocument.EnumerateObject(), jsonSerializerOptions) is Document found)
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
                transaction != null &&
                doc.TryGetProperty("transaction", out JsonElement transactionElement) &&
                transactionElement.GetString() is string transactionToken)
            {
                transaction.Token = transactionToken;
            }
        }

        return response.Concat(new QueryDocumentResult(
            foundDocuments,
            skippedResults,
            skippedReadTime,
            page,
            pageSize,
            response,
            (pageNum, ct) =>
            {
                return QueryDocumentPage(
                    response,
                    pageNum,
                    objType,
                    fromQuery,
                    firstFromQuery,
                    pageSize,
                    0,
                    documentReference,
                    selectQuery,
                    filterQuery,
                    orderQuery,
                    transaction,
                    cacheDocuments,
                    authorization,
                    jsonSerializerOptions,
                    cancellationToken);
            }));
    }

    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    private async Task<HttpResponse<QueryDocumentResult<T>>> QueryDocumentPage<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>(
        HttpResponse<QueryDocumentResult<T>> response,
        int page,
        FromQuery.Builder fromQuery,
        FromQuery firstFromQuery,
        int pageSize,
        int offset,
        DocumentReference? documentReference,
        SelectQuery.Builder? selectQuery,
        FilterQuery.Builder? filterQuery,
        OrderQuery.Builder? orderQuery,
        Transaction? transaction,
        IEnumerable<Document>? cacheDocuments,
        IAuthorization? authorization,
        JsonSerializerOptions jsonSerializerOptions,
        CancellationToken cancellationToken)
        where T : class
    {
        Type objType = typeof(T);
        List<(string fieldPath, Direction direction)>? orderBys = null;
        List<object>? startCursors = null;
        List<object>? endCursors = null;
        bool? isStartAfter = null;
        bool? isEndBefore = null;

        if (orderQuery != null)
        {
            orderBys = new();
            startCursors = new();
            endCursors = new();
            isStartAfter = orderQuery.IsStartAfter;
            isEndBefore = orderQuery.IsEndBefore;

            bool hasDocumentName = false;
            foreach (var query in orderQuery.OrderQuery)
            {
                var documentFieldPath = DocumentFieldHelpers.GetDocumentFieldPath(query.ModelType, query.NamePath, jsonSerializerOptions);
                var lastDocumentFieldPath = documentFieldPath.LastOrDefault()!;

                if (documentFieldPath[0].DocumentFieldName == DocumentFieldHelpers.DocumentName)
                {
                    if (hasDocumentName)
                    {
                        throw new ArgumentException($"\"{nameof(orderQuery)}\" must only contain a single document name.");
                    }
                    hasDocumentName = true;
                }
                else if (hasDocumentName)
                {
                    throw new ArgumentException($"\"{nameof(orderQuery)}\" with document name must be place on the end of the query.");
                }

                orderBys.Add((string.Join(".", documentFieldPath.Select(i => i.DocumentFieldName)), query.Direction));
                startCursors.Add(query.StartPosition ?? DocumentFieldHelpers.GetLimitValue(lastDocumentFieldPath.Type, query.Direction == Direction.Ascending));
                endCursors.Add(query.EndPosition ?? DocumentFieldHelpers.GetLimitValue(lastDocumentFieldPath.Type, query.Direction == Direction.Ascending));
            }
        }

        var (jsonDocument, queryDocumentResponse) = await ExecuteQueryDocument(
            page,
            pageSize,
            offset,
            documentReference,
            fromQuery,
            firstFromQuery,
            selectQuery,
            filterQuery,
            orderBys,
            startCursors,
            endCursors,
            isStartAfter,
            isEndBefore,
            namePath =>
            {
                bool hasDocumentName = false;
                var documentFieldPath = DocumentFieldHelpers.GetDocumentFieldPath(objType, namePath, jsonSerializerOptions);

                if (documentFieldPath[0].DocumentFieldName == DocumentFieldHelpers.DocumentName)
                {
                    if (hasDocumentName)
                    {
                        throw new ArgumentException($"\"{nameof(orderQuery)}\" must only contain a single document name.");
                    }
                    hasDocumentName = true;
                }
                else if (hasDocumentName)
                {
                    throw new ArgumentException($"\"{nameof(orderQuery)}\" with document name must be place on the end of the query.");
                }

                return string.Join(".", documentFieldPath.Select(i => i.DocumentFieldName));
            },
            transaction,
            authorization,
            jsonSerializerOptions,
            cancellationToken);
        response.Concat(queryDocumentResponse);
        if (queryDocumentResponse.IsError || jsonDocument == null)
        {
            return response;
        }

        List<DocumentTimestamp<T>> foundDocuments = new();
        int? skippedResults = null;
        DateTimeOffset? skippedReadTime = null;

        foreach (var doc in jsonDocument.RootElement.EnumerateArray())
        {
            if (doc.TryGetProperty("readTime", out JsonElement readTimeProperty) &&
                readTimeProperty.GetDateTimeOffset() is DateTimeOffset readTime)
            {
                DocumentReference? parsedDocumentReference = null;
                Document<T>? parsedDocument = null;
                T? parsedModel = null;
                if (doc.TryGetProperty("document", out JsonElement foundPropertyDocument))
                {
                    if (foundPropertyDocument.TryGetProperty("name", out JsonElement foundNameProperty) &&
                        DocumentReference.Parse(App, foundNameProperty, jsonSerializerOptions) is DocumentReference docRef)
                    {
                        parsedDocumentReference = docRef;

                        if (cacheDocuments != null &&
                            cacheDocuments.FirstOrDefault(i => i.Reference.Equals(docRef)) is Document<T> foundDocument)
                        {
                            parsedDocument = foundDocument;
                            parsedModel = foundDocument.Model;
                        }
                    }

                    if (Document<T>.Parse(App, documentReference, parsedModel, parsedDocument, foundPropertyDocument.EnumerateObject(), jsonSerializerOptions) is Document<T> found)
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
                transaction != null &&
                doc.TryGetProperty("transaction", out JsonElement transactionElement) &&
                transactionElement.GetString() is string transactionToken)
            {
                transaction.Token = transactionToken;
            }
        }

        return response.Concat(new QueryDocumentResult<T>(
            foundDocuments,
            skippedResults,
            skippedReadTime,
            page,
            pageSize,
            response,
            (pageNum, ct) =>
            {
                return QueryDocumentPage<T>(
                    response,
                    pageNum,
                    fromQuery,
                    firstFromQuery,
                    pageSize,
                    0,
                    documentReference,
                    selectQuery,
                    filterQuery,
                    orderQuery,
                    transaction,
                    cacheDocuments,
                    authorization,
                    jsonSerializerOptions,
                    cancellationToken);
            }));
    }
}
