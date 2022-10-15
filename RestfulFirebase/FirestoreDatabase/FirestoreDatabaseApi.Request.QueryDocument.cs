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
    internal async Task<(JsonDocument?, HttpResponse)> ExecuteQueryDocument<TQuery>(
        int page,
        int offset,
        StructuredQuery<TQuery> query,
        List<StructuredCursor> startCursor,
        Transaction? transaction,
        IAuthorization? authorization,
        JsonSerializerOptions jsonSerializerOptions,
        CancellationToken cancellationToken)
        where TQuery : BaseQuery<TQuery>
    {
        string url;
        if (query.Query.DocumentReference != null)
        {
            url = query.Query.DocumentReference.BuildUrl(App.Config.ProjectId, ":runQuery");
        }
        else
        {
            url =
                $"{FirestoreDatabaseV1Endpoint}/" +
                $"{string.Format(FirestoreDatabaseDocumentsEndpoint, App.Config.ProjectId, ":runQuery")}";
        }

        using MemoryStream stream = new();
        Utf8JsonWriter writer = new(stream);

        writer.WriteStartObject();
        writer.WritePropertyName("structuredQuery");
        writer.WriteStartObject();
        writer.WritePropertyName("from");
        writer.WriteStartArray();
        foreach (var from in query.From)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("collectionId");
            writer.WriteStringValue(from.FromQuery.CollectionId);
            writer.WritePropertyName("allDescendants");
            writer.WriteBooleanValue(from.FromQuery.AllDescendants);
            writer.WriteEndObject();
        }
        writer.WriteEndArray();
        if (query.Select.Count != 0)
        {
            writer.WritePropertyName("select");
            writer.WriteStartObject();
            writer.WritePropertyName("fields");
            writer.WriteStartArray();
            foreach (var select in query.Select)
            {
                writer.WriteStartObject();
                writer.WritePropertyName("fieldPath");
                writer.WriteStringValue(select.DocumentFieldPath);
                writer.WriteEndObject();
            }
            writer.WriteEndArray();
            writer.WriteEndObject();
        }
        if (query.Where.Count != 0)
        {
            writer.WritePropertyName("where");
            writer.WriteStartObject();
            writer.WritePropertyName("compositeFilter");
            writer.WriteStartObject();
            writer.WritePropertyName("op");
            writer.WriteStringValue("AND");
            writer.WritePropertyName("filters");
            writer.WriteStartArray();
            foreach (var filter in query.Where)
            {
                switch (filter.FilterQuery)
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
                        writer.WriteStringValue(filter.DocumentFieldPath);
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
                        writer.WriteStringValue(filter.DocumentFieldPath);
                        writer.WriteEndObject();
                        writer.WritePropertyName("value");
                        ModelBuilderHelpers.BuildUtf8JsonWriterObject(App.Config, writer, fieldFilter.Value?.GetType(), fieldFilter.Value, jsonSerializerOptions, null, null);
                        writer.WriteEndObject();
                        writer.WriteEndObject();

                        break;
                    default:

                        throw new NotImplementedException($"\"{filter.GetType()}\" filter is not implemented.");
                }
            }
            writer.WriteEndArray();
            writer.WriteEndObject();
            writer.WriteEndObject();
        }

        if (query.OrderBy.Count != 0)
        {
            writer.WritePropertyName("orderBy");
            writer.WriteStartArray();
            foreach (var orderBy in query.OrderBy)
            {
                writer.WriteStartObject();
                writer.WritePropertyName("field");
                writer.WriteStartObject();
                writer.WritePropertyName("fieldPath");
                writer.WriteStringValue(orderBy.DocumentFieldPath);
                writer.WriteEndObject();
                writer.WritePropertyName("direction");
                writer.WriteStringValue(orderBy.OrderByQuery.Direction == Direction.Ascending ? "ASCENDING" : "DESCENDING");
                writer.WriteEndObject();
            }
            writer.WriteEndArray();
        }

        if (startCursor.Count != 0)
        {
            writer.WritePropertyName("startAt");
            writer.WriteStartObject();
            writer.WritePropertyName("values");
            writer.WriteStartArray();
            foreach (var cursor in startCursor)
            {
                ModelBuilderHelpers.BuildUtf8JsonWriterObject(App.Config, writer, cursor.ValueType, cursor.Value, jsonSerializerOptions, null, null);
            }
            writer.WriteEndArray();
            writer.WritePropertyName("before");
            writer.WriteBooleanValue(!query.Query.IsStartAfter);
            writer.WriteEndObject();
        }

        if (query.EndCursor.Count != 0)
        {
            writer.WritePropertyName("endAt");
            writer.WriteStartObject();
            writer.WritePropertyName("values");
            writer.WriteStartArray();
            foreach (var cursor in query.EndCursor)
            {
                ModelBuilderHelpers.BuildUtf8JsonWriterObject(App.Config, writer, cursor.ValueType, cursor.Value, jsonSerializerOptions, null, null);
            }
            writer.WriteEndArray();
            writer.WritePropertyName("before");
            writer.WriteBooleanValue(query.Query.IsEndBefore);
            writer.WriteEndObject();
        }

        writer.WritePropertyName("offset");
        writer.WriteNumberValue(offset);
        writer.WritePropertyName("limit");
        writer.WriteNumberValue(query.Query.PageSize);
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
        using Stream contentStream = await lastHttpTransaction.ResponseMessage.Content.ReadAsStreamAsync(cancellationToken);
#else
        using Stream contentStream = await lastHttpTransaction.ResponseMessage.Content.ReadAsStreamAsync();
#endif

        return (await JsonDocument.ParseAsync(contentStream, cancellationToken: cancellationToken), response);
    }

    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    private async Task<HttpResponse<QueryDocumentResult>> QueryDocumentPage<TQuery>(
        HttpResponse<QueryDocumentResult> response,
        StructuredQuery<TQuery> query,
        List<StructuredCursor> startCursor,
        int page,
        int offset,
        Transaction? transaction,
        IEnumerable<Document>? cacheDocuments,
        IAuthorization? authorization,
        JsonSerializerOptions jsonSerializerOptions,
        CancellationToken cancellationToken)
        where TQuery : BaseQuery<TQuery>
    {
        var (jsonDocument, queryDocumentResponse) = await ExecuteQueryDocument(
            page,
            offset,
            query,
            startCursor,
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

                    if (Document.Parse(App, parsedDocumentReference, parsedModel?.GetType(), parsedModel, parsedDocument, foundPropertyDocument.EnumerateObject(), jsonSerializerOptions) is Document found)
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

        return response.Append(new QueryDocumentResult(
            foundDocuments,
            skippedResults,
            skippedReadTime,
            page,
            query.Query.PageSize,
            response,
            (pageNum, ct) =>
            {
                return QueryDocumentPage(
                    response,
                    query,
                    startCursor,
                    pageNum,
                    0,
                    transaction,
                    cacheDocuments,
                    authorization,
                    jsonSerializerOptions,
                    ct);
            }));
    }

    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    private async Task<HttpResponse<QueryDocumentResult<T>>> QueryDocumentPage<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T, TQuery>(
        HttpResponse<QueryDocumentResult<T>> response,
        StructuredQuery<TQuery> query,
        List<StructuredCursor> startCursor,
        int page,
        int offset,
        Transaction? transaction,
        IEnumerable<Document>? cacheDocuments,
        IAuthorization? authorization,
        JsonSerializerOptions jsonSerializerOptions,
        CancellationToken cancellationToken)
        where T : class
        where TQuery : BaseQuery<TQuery>
    {
        var (jsonDocument, queryDocumentResponse) = await ExecuteQueryDocument(
            page,
            offset,
            query,
            startCursor,
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

                    if (Document<T>.Parse(App, parsedDocumentReference, parsedModel, parsedDocument, foundPropertyDocument.EnumerateObject(), jsonSerializerOptions) is Document<T> found)
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

        return response.Append(new QueryDocumentResult<T>(
            foundDocuments,
            skippedResults,
            skippedReadTime,
            page,
            query.Query.PageSize,
            response,
            (pageNum, ct) =>
            {
                return QueryDocumentPage(
                    response,
                    query,
                    startCursor,
                    pageNum,
                    0,
                    transaction,
                    cacheDocuments,
                    authorization,
                    jsonSerializerOptions,
                    ct);
            }));
    }

    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    internal async Task<HttpResponse<QueryDocumentResult>> QueryDocument<TQuery>(
        BaseQuery<TQuery> query,
        IEnumerable<Document>? cacheDocuments = default,
        Transaction? transaction = default,
        IAuthorization? authorization = default,
        CancellationToken cancellationToken = default)
        where TQuery : BaseQuery<TQuery>
    {
        JsonSerializerOptions jsonSerializerOptions = ConfigureJsonSerializerOption();

        StructuredQuery<TQuery> structuredQuery = new(query);

        foreach (var fromQuery in query.FromQuery)
        {
            structuredQuery.From.Add(new(fromQuery));
        }
        foreach (var selectQuery in query.SelectQuery)
        {
            var documentFieldPath = DocumentFieldHelpers.GetDocumentFieldPath(query.ModelType, selectQuery.NamePath, jsonSerializerOptions);

            string fieldPath = string.Join(".", documentFieldPath.Select(i => i.DocumentFieldName));

            structuredQuery.Select.Add(new(selectQuery, fieldPath));
        }
        foreach (var whereQuery in query.WhereQuery)
        {
            var documentFieldPath = DocumentFieldHelpers.GetDocumentFieldPath(query.ModelType, whereQuery.NamePath, jsonSerializerOptions);

            string fieldPath = string.Join(".", documentFieldPath.Select(i => i.DocumentFieldName));

            structuredQuery.Where.Add(new(whereQuery, fieldPath));
        }
        foreach (var orderByQuery in query.OrderByQuery)
        {
            var documentFieldPath = DocumentFieldHelpers.GetDocumentFieldPath(query.ModelType, orderByQuery.NamePath, jsonSerializerOptions);

            string fieldPath = string.Join(".", documentFieldPath.Select(i => i.DocumentFieldName));

            structuredQuery.OrderBy.Add(new(orderByQuery, fieldPath));
        }

        return await QueryDocumentPage(
            new(),
            structuredQuery,
            structuredQuery.StartCursor,
            0,
            query.SkipPage * query.PageSize,
            transaction,
            cacheDocuments,
            authorization,
            jsonSerializerOptions,
            cancellationToken);
    }

    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    internal async Task<HttpResponse<QueryDocumentResult<T>>> QueryDocument<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T, TQuery>(
        BaseQuery<TQuery> query,
        IEnumerable<Document>? cacheDocuments = default,
        Transaction? transaction = default,
        IAuthorization? authorization = default,
        CancellationToken cancellationToken = default)
        where T : class
        where TQuery : BaseQuery<TQuery>
    {
        JsonSerializerOptions jsonSerializerOptions = ConfigureJsonSerializerOption();

        StructuredQuery<TQuery> structuredQuery = new(query);

        return await QueryDocumentPage<T, TQuery>(
            new(),
            structuredQuery,
            structuredQuery.StartCursor,
            0,
            query.SkipPage * query.PageSize,
            transaction,
            cacheDocuments,
            authorization,
            jsonSerializerOptions,
            cancellationToken);
    }
}
