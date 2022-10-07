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
        BaseQuery<TQuery> query,
        Transaction? transaction,
        IAuthorization? authorization,
        JsonSerializerOptions jsonSerializerOptions,
        CancellationToken cancellationToken)
        where TQuery : BaseQuery<TQuery>
    {
        string url;
        if (query.DocumentReference != null)
        {
            url = query.DocumentReference.BuildUrl(App.Config.ProjectId, ":runQuery");
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
        foreach (var from in query.FromQuery)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("collectionId");
            writer.WriteStringValue(from.CollectionId);
            writer.WritePropertyName("allDescendants");
            writer.WriteBooleanValue(from.AllDescendants);
            writer.WriteEndObject();
        }
        writer.WriteEndArray();
        if (query.SelectQuery.Count != 0)
        {
            writer.WritePropertyName("select");
            writer.WriteStartObject();
            writer.WritePropertyName("fields");
            writer.WriteStartArray();
            foreach (var select in query.SelectQuery)
            {
                writer.WriteStartObject();
                writer.WritePropertyName("fieldPath");
                writer.WriteStringValue(select.DocumentFieldPath);
                writer.WriteEndObject();
            }
            writer.WriteEndArray();
            writer.WriteEndObject();
        }
        if (query.WhereQuery.Count != 0)
        {
            writer.WritePropertyName("where");
            writer.WriteStartObject();
            writer.WritePropertyName("compositeFilter");
            writer.WriteStartObject();
            writer.WritePropertyName("op");
            writer.WriteStringValue("AND");
            writer.WritePropertyName("filters");
            writer.WriteStartArray();
            foreach (var filter in query.WhereQuery)
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
                        writer.WriteStringValue(unaryFilter.DocumentFieldPath);
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
                        writer.WriteStringValue(fieldFilter.DocumentFieldPath);
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

        if (query.OrderByQuery.Count != 0)
        {
            writer.WritePropertyName("orderBy");
            writer.WriteStartArray();
            foreach (var orderBy in query.OrderByQuery)
            {
                writer.WriteStartObject();
                writer.WritePropertyName("field");
                writer.WriteStartObject();
                writer.WritePropertyName("fieldPath");
                writer.WriteStringValue(orderBy.DocumentFieldPath);
                writer.WriteEndObject();
                writer.WritePropertyName("direction");
                writer.WriteStringValue(orderBy.Direction == Direction.Ascending ? "ASCENDING" : "DESCENDING");
                writer.WriteEndObject();
            }
            writer.WriteEndArray();
        }

        if (query.StartCursorQuery.Count != 0)
        {
            writer.WritePropertyName("startAt");
            writer.WriteStartObject();
            writer.WritePropertyName("values");
            writer.WriteStartArray();
            foreach (var cursor in query.StartCursorQuery)
            {
                Type? objType = cursor.Value?.GetType();
                object? obj;

                if (cursor.Value is Document document)
                {
                    obj = document.Reference;
                }
                else if (cursor.Value is DocumentTimestamp documentTimestamp)
                {
                    obj = documentTimestamp.Document.Reference;
                }
                else if (cursor.Value is DocumentReferenceTimestamp documentReferenceTimestamp)
                {
                    obj = documentReferenceTimestamp.Reference;
                }
                else
                {
                    obj = cursor.Value;
                }

                ModelBuilderHelpers.BuildUtf8JsonWriterObject(App.Config, writer, objType, obj, jsonSerializerOptions, null, null);
            }
            writer.WriteEndArray();
            writer.WritePropertyName("before");
            writer.WriteBooleanValue(!query.IsStartAfter);
            writer.WriteEndObject();
        }

        if (query.EndCursorQuery.Count != 0)
        {
            writer.WritePropertyName("endAt");
            writer.WriteStartObject();
            writer.WritePropertyName("values");
            writer.WriteStartArray();
            foreach (var cursor in query.EndCursorQuery)
            {
                Type? objType = cursor.Value?.GetType();
                object? obj;

                if (cursor.Value is Document document)
                {
                    obj = document.Reference;
                }
                else if (cursor.Value is DocumentTimestamp documentTimestamp)
                {
                    obj = documentTimestamp.Document.Reference;
                }
                else if (cursor.Value is DocumentReferenceTimestamp documentReferenceTimestamp)
                {
                    obj = documentReferenceTimestamp.Reference;
                }
                else
                {
                    obj = cursor.Value;
                }

                ModelBuilderHelpers.BuildUtf8JsonWriterObject(App.Config, writer, objType, obj, jsonSerializerOptions, null, null);
            }
            writer.WriteEndArray();
            writer.WritePropertyName("before");
            writer.WriteBooleanValue(query.IsEndBefore);
            writer.WriteEndObject();
        }

        writer.WritePropertyName("offset");
        writer.WriteNumberValue(offset);
        writer.WritePropertyName("limit");
        writer.WriteNumberValue(query.PageSize);
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
    internal async Task<HttpResponse<QueryDocumentResult>> QueryDocument<TQuery>(
        BaseQuery<TQuery> query,
        IEnumerable<Document>? cacheDocuments = default,
        Transaction? transaction = default,
        IAuthorization? authorization = default,
        CancellationToken cancellationToken = default)
        where TQuery : BaseQuery<TQuery>
    {
        JsonSerializerOptions jsonSerializerOptions = ConfigureJsonSerializerOption();

        return await QueryDocumentPage(
            new(),
            query,
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

        return await QueryDocumentPage<T, TQuery>(
            new(),
            query,
            0,
            query.SkipPage * query.PageSize,
            transaction,
            cacheDocuments,
            authorization,
            jsonSerializerOptions,
            cancellationToken);
    }

    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    private async Task<HttpResponse<QueryDocumentResult>> QueryDocumentPage<TQuery>(
        HttpResponse<QueryDocumentResult> response,
        BaseQuery<TQuery> query,
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

                    if (Document.Parse(App, query.DocumentReference, parsedModel?.GetType(), parsedModel, parsedDocument, foundPropertyDocument.EnumerateObject(), jsonSerializerOptions) is Document found)
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
            query.PageSize,
            response,
            (pageNum, ct) =>
            {
                return QueryDocumentPage(
                    response,
                    query,
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
        BaseQuery<TQuery> query,
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

                    if (Document<T>.Parse(App, query.DocumentReference, parsedModel, parsedDocument, foundPropertyDocument.EnumerateObject(), jsonSerializerOptions) is Document<T> found)
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
            query.PageSize,
            response,
            (pageNum, ct) =>
            {
                return QueryDocumentPage(
                    response,
                    query,
                    pageNum,
                    0,
                    transaction,
                    cacheDocuments,
                    authorization,
                    jsonSerializerOptions,
                    ct);
            }));
    }
}
