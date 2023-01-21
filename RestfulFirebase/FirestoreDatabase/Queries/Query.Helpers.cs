using System;
using System.Threading.Tasks;
using System.IO;
using System.Text.Json;
using System.Diagnostics.CodeAnalysis;
using RestfulFirebase.Common.Utilities;
using System.Collections.Generic;
using RestfulFirebase.FirestoreDatabase.References;
using System.Linq;
using RestfulFirebase.FirestoreDatabase.Models;
using RestfulFirebase.FirestoreDatabase.Enums;
using RestfulFirebase.FirestoreDatabase.Utilities;
using System.Data;
using System.Threading;
using RestfulFirebase.Common.Http;

namespace RestfulFirebase.FirestoreDatabase.Queries;

public abstract partial class Query
{
    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    internal async Task<(JsonDocument?, HttpResponse)> ExecuteQueryDocument(
        int page,
        int offset,
        StructuredQuery query,
        JsonSerializerOptions jsonSerializerOptions,
        CancellationToken cancellationToken)
    {
        string url;
        if (query.DocumentReference != null)
        {
            url = query.DocumentReference.BuildUrl(App.Config.ProjectId, ":runQuery");
        }
        else
        {
            url =
                $"{FirestoreDatabaseApi.FirestoreDatabaseV1Endpoint}/" +
                $"{string.Format(FirestoreDatabaseApi.FirestoreDatabaseDocumentsEndpoint, App.Config.ProjectId, ":runQuery")}";
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
                        ModelBuilderHelpers.BuildUtf8JsonWriterObject(App.Config, writer, fieldFilter.Value?.GetType(), fieldFilter.Value, jsonSerializerOptions,
                            () =>
                            {
                                writer.WritePropertyName("value");
                            },
                            null);
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

        if (query.StartCursor.Count != 0)
        {
            writer.WritePropertyName("startAt");
            writer.WriteStartObject();
            writer.WritePropertyName("values");
            writer.WriteStartArray();
            foreach (var cursor in query.StartCursor)
            {
                ModelBuilderHelpers.BuildUtf8JsonWriterObject(App.Config, writer, cursor.ValueType, cursor.Value, jsonSerializerOptions, null, null);
            }
            writer.WriteEndArray();
            writer.WritePropertyName("before");
            writer.WriteBooleanValue(!query.IsStartAfter);
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
            writer.WriteBooleanValue(query.IsEndBefore);
            writer.WriteEndObject();
        }

        writer.WritePropertyName("offset");
        writer.WriteNumberValue(offset);
        writer.WritePropertyName("limit");
        writer.WriteNumberValue(query.SizeOfPages);
        writer.WriteEndObject();
        if (TransactionUsed != null)
        {
            if (TransactionUsed.Token == null)
            {
                writer.WritePropertyName("newTransaction");
                FirestoreDatabaseApi.BuildTransactionOption(writer, TransactionUsed);
            }
            else
            {
                FirestoreDatabaseApi.BuildTransaction(writer, TransactionUsed);
            }
        }
        writer.WriteEndObject();

        await writer.FlushAsync(cancellationToken);

        var response = await App.FirestoreDatabase.ExecutePost(AuthorizationUsed, stream, url, cancellationToken);
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

    internal async Task<HttpResponse<Document?>> GetLatestDocument(
        DocumentReference? docRef,
        CancellationToken cancellationToken)
    {
        HttpResponse<Document?> response = new();

        if (docRef != null)
        {
            var getDocumentResponse = await App.FirestoreDatabase.Fetch()
                .DocumentReference(docRef)
                .Cache(CacheDocuments)
                .Transaction(TransactionUsed)
                .Authorization(AuthorizationUsed)
                .RunSingle(cancellationToken);
            response.Append(getDocumentResponse);
            if (getDocumentResponse.IsError || getDocumentResponse.Result?.Found == null)
            {
                return response;
            }
            response.Append(getDocumentResponse.Result?.Found?.Document);
        }

        return response;
    }

    internal async Task<(CursorQuery? cursorQuery, HttpResponse<Document?> response)> GetLatestDocument(
        IEnumerable<CursorQuery> cursorQueries,
        CancellationToken cancellationToken)
    {
        HttpResponse<Document?> response = new();

        var cursorQueryDoc = cursorQueries.LastOrDefault(i =>
            i.Value is Document ||
            i.Value is DocumentTimestamp ||
            i.Value is DocumentReference ||
            i.Value is DocumentReferenceTimestamp);

        DocumentReference? docRef = null;

        if (cursorQueryDoc?.Value is Document document)
        {
            if (TransactionUsed != null)
            {
                response.Append(document);
                return (cursorQueryDoc, response);
            }
            docRef = document.Reference;
        }
        else if (cursorQueryDoc?.Value is DocumentTimestamp documentTimestamp)
        {
            if (TransactionUsed != null)
            {
                response.Append(documentTimestamp.Document);
                return (cursorQueryDoc, response);
            }
            docRef = documentTimestamp.Document.Reference;
        }
        else if (cursorQueryDoc?.Value is DocumentReference documentReference)
        {
            docRef = documentReference;
        }
        else if (cursorQueryDoc?.Value is DocumentReferenceTimestamp documentReferenceTimestamp)
        {
            docRef = documentReferenceTimestamp.Reference;
        }

        var getDocumentResponse = await GetLatestDocument(docRef, cancellationToken);
        response.Append(getDocumentResponse);

        return (cursorQueryDoc, response);
    }

    internal static void BuildOrderCursor(
        StructuredQuery structuredQuery,
        Document? startDoc,
        Document? endDoc,
        JsonSerializerOptions jsonSerializerOptions)
    {
        if (startDoc != null)
        {
            structuredQuery.StartCursor.Clear();
        }
        if (endDoc != null)
        {
            structuredQuery.EndCursor.Clear();
        }

        bool orderByHasDocumentNameIndicator = false;
        foreach (var orderByQuery in structuredQuery.Query.OrderByQuery)
        {
            if (orderByHasDocumentNameIndicator)
            {
                ArgumentException.Throw($"OrderBy query has \"__name__\" and must be placed at the end.");
            }

            string? fieldPath = null;
            if (orderByQuery.NamePath.Length == 1 && orderByQuery.NamePath[0] == DocumentFieldHelpers.DocumentName)
            {
                fieldPath = DocumentFieldHelpers.DocumentName;
                orderByHasDocumentNameIndicator = true;
            }
            else if (!orderByQuery.IsNamePathAPropertyPath)
            {
                fieldPath = string.Join(".", orderByQuery.NamePath);
            }
            else if (structuredQuery.ModelType != null)
            {
                var documentFieldPath = ModelFieldHelpers.GetModelFieldPath(structuredQuery.ModelType, orderByQuery.NamePath, jsonSerializerOptions);
                fieldPath = string.Join(".", documentFieldPath.Select(i => i.ModelFieldName));
            }
            else
            {
                ArgumentException.Throw($"OrderBy query with property path enabled requires a query with types");
            }

            if (startDoc != null && !orderByHasDocumentNameIndicator)
            {
                startDoc.Fields.TryGetValue(fieldPath, out object? startDocValue);
                structuredQuery.StartCursor.Add(new(new(startDocValue), startDocValue?.GetType(), startDocValue));
            }
            if (endDoc != null && !orderByHasDocumentNameIndicator)
            {
                endDoc.Fields.TryGetValue(fieldPath, out object? endDocValue);
                structuredQuery.EndCursor.Add(new(new(endDocValue), endDocValue?.GetType(), endDocValue));
            }

            structuredQuery.OrderBy.Add(new(orderByQuery, fieldPath));
        }

        if (!orderByHasDocumentNameIndicator)
        {
            structuredQuery.OrderBy.Add(new(new(new string[] { DocumentFieldHelpers.DocumentName }, false, structuredQuery.OrderBy.LastOrDefault()?.OrderByQuery.Direction ?? Direction.Ascending), DocumentFieldHelpers.DocumentName));
        }

        if (startDoc != null)
        {
            structuredQuery.StartCursor.Add(new(new(startDoc.Reference), typeof(DocumentReference), startDoc.Reference));
        }
        if (endDoc != null)
        {
            structuredQuery.EndCursor.Add(new(new(endDoc.Reference), typeof(DocumentReference), endDoc.Reference));
        }
    }

    internal async Task<HttpResponse<StructuredQuery>> BuildStartingStructureQuery(
        JsonSerializerOptions jsonSerializerOptions,
        CancellationToken cancellationToken)
    {
        HttpResponse<StructuredQuery> response = new();

        StructuredQuery structuredQuery = new(this);

        foreach (var fromQuery in FromQuery)
        {
            structuredQuery.From.Add(new(fromQuery));
        }

        bool selectHasDocumentNameIndicator = false;
        foreach (var selectQuery in SelectQuery)
        {
            if (selectHasDocumentNameIndicator && SelectQuery.Count != 1)
            {
                ArgumentException.Throw($"Select query has \"__name__\" indicator and should not contain any other select query.");
            }

            string? fieldPath = null;
            if (selectQuery.NamePath.Length == 1 && selectQuery.NamePath[0] == DocumentFieldHelpers.DocumentName)
            {
                fieldPath = DocumentFieldHelpers.DocumentName;
                selectHasDocumentNameIndicator = true;
            }
            else if (!selectQuery.IsNamePathAPropertyPath)
            {
                fieldPath = string.Join(".", selectQuery.NamePath);
            }
            else if (ModelType != null)
            {
                var documentFieldPath = ModelFieldHelpers.GetModelFieldPath(ModelType, selectQuery.NamePath, jsonSerializerOptions);
                fieldPath = string.Join(".", documentFieldPath.Select(i => i.ModelFieldName));
            }
            else
            {
                ArgumentException.Throw($"Select query with property path enabled requires a query with types");
            }

            structuredQuery.Select.Add(new(selectQuery, fieldPath));
        }

        foreach (var whereQuery in WhereQuery)
        {
            string? fieldPath = null;
            if (whereQuery.NamePath.Length == 1 && whereQuery.NamePath[0] == DocumentFieldHelpers.DocumentName)
            {
                fieldPath = DocumentFieldHelpers.DocumentName;
            }
            else if (!whereQuery.IsNamePathAPropertyPath)
            {
                fieldPath = string.Join(".", whereQuery.NamePath);
            }
            else if (ModelType != null)
            {
                var documentFieldPath = ModelFieldHelpers.GetModelFieldPath(ModelType, whereQuery.NamePath, jsonSerializerOptions);
                fieldPath = string.Join(".", documentFieldPath.Select(i => i.ModelFieldName));
            }
            else
            {
                ArgumentException.Throw($"Where query with property path enabled requires a query with types");
            }

            structuredQuery.Where.Add(new(whereQuery, fieldPath));
        }

        var (startCursorRef, getLatestStartCursorDoc) = await GetLatestDocument(StartCursorQuery, cancellationToken);
        response.Append(getLatestStartCursorDoc);
        if (getLatestStartCursorDoc.IsError)
        {
            return response;
        }
        Document? startDoc = getLatestStartCursorDoc.Result;

        var (endCursorRef, getLatestEndCursorDoc) = await GetLatestDocument(EndCursorQuery, cancellationToken);
        response.Append(getLatestEndCursorDoc);
        if (getLatestEndCursorDoc.IsError)
        {
            return response;
        }
        Document? endDoc = getLatestEndCursorDoc.Result;

        BuildOrderCursor(structuredQuery, startDoc, endDoc, jsonSerializerOptions);

        int startCursorIndex = 0;
        int endCursorIndex = 0;

        foreach (var startCursorQuery in StartCursorQuery)
        {
            if (startCursorQuery == startCursorRef)
            {
                continue;
            }

            Type? objType;
            object? obj;
            if (startCursorQuery.Value is Document document)
            {
                obj = document.Reference;
                objType = document.Reference.GetType();
            }
            else if (startCursorQuery.Value is DocumentTimestamp documentTimestamp)
            {
                obj = documentTimestamp.Document.Reference;
                objType = documentTimestamp.Document.Reference.GetType();
            }
            else if (startCursorQuery.Value is DocumentReferenceTimestamp documentReferenceTimestamp)
            {
                obj = documentReferenceTimestamp.Reference;
                objType = documentReferenceTimestamp.Reference.GetType();
            }
            else
            {
                obj = startCursorQuery.Value;
                objType = startCursorQuery.Value?.GetType();
            }

            if (structuredQuery.StartCursor.Count < startCursorIndex)
            {
                structuredQuery.StartCursor.Add(new(startCursorQuery, objType, obj));
            }
            else
            {
                structuredQuery.StartCursor[startCursorIndex] = new(startCursorQuery, objType, obj);
            }

            startCursorIndex++;
        }

        foreach (var endCursorQuery in EndCursorQuery)
        {
            if (endCursorQuery == endCursorRef)
            {
                continue;
            }

            Type? objType;
            object? obj;
            if (endCursorQuery.Value is Document document)
            {
                obj = document.Reference;
                objType = document.Reference.GetType();
            }
            else if (endCursorQuery.Value is DocumentTimestamp documentTimestamp)
            {
                obj = documentTimestamp.Document.Reference;
                objType = documentTimestamp.Document.Reference.GetType();
            }
            else if (endCursorQuery.Value is DocumentReferenceTimestamp documentReferenceTimestamp)
            {
                obj = documentReferenceTimestamp.Reference;
                objType = documentReferenceTimestamp.Reference.GetType();
            }
            else
            {
                obj = endCursorQuery.Value;
                objType = endCursorQuery.Value?.GetType();
            }

            if (structuredQuery.EndCursor.Count < startCursorIndex)
            {
                structuredQuery.EndCursor.Add(new(endCursorQuery, objType, obj));
            }
            else
            {
                structuredQuery.EndCursor[startCursorIndex] = new(endCursorQuery, objType, obj);
            }

            endCursorIndex++;
        }

        response.Append(structuredQuery);

        return response;
    }

    internal async Task<HttpResponse<StructuredQuery>> BuildStructureQuery(
        StructuredQuery query,
        Document? startDoc,
        JsonSerializerOptions jsonSerializerOptions,
        CancellationToken cancellationToken)
    {
        HttpResponse<StructuredQuery> response = new();

        StructuredQuery structuredQuery = new(query);

        structuredQuery.OrderBy.Clear();
        structuredQuery.StartCursor.Clear();
        structuredQuery.IsStartAfter = true;

        if (startDoc == null)
        {
            var (_, getLatestStartCursorDoc) = await GetLatestDocument(query.Query.StartCursorQuery, cancellationToken);
            response.Append(getLatestStartCursorDoc);
            if (getLatestStartCursorDoc.IsError)
            {
                return response;
            }
            startDoc = getLatestStartCursorDoc.Result;
        }
        else
        {
            var getLatestStartCursorDoc = await GetLatestDocument(startDoc.Reference, cancellationToken);
            response.Append(getLatestStartCursorDoc);
            if (getLatestStartCursorDoc.IsError)
            {
                return response;
            }
            startDoc = getLatestStartCursorDoc.Result;
        }

        BuildOrderCursor(structuredQuery, startDoc, null, jsonSerializerOptions);

        response.Append(structuredQuery);

        return response;
    }

    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    internal async Task<HttpResponse<QueryDocumentResult>> QueryDocumentPage(
        HttpResponse<QueryDocumentResult> response,
        Task<HttpResponse<StructuredQuery>> queryTask,
        int page,
        int offset,
        JsonSerializerOptions jsonSerializerOptions,
        CancellationToken cancellationToken)
    {
        var queryResponse = await queryTask;
        response.Append(queryResponse);
        if (queryResponse.IsError)
        {
            return response;
        }

        var query = queryResponse.Result;

        var (jsonDocument, queryDocumentResponse) = await ExecuteQueryDocument(
            page,
            offset,
            query,
            jsonSerializerOptions,
            cancellationToken);
        response.Append(queryDocumentResponse);
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

                        if (CacheDocuments.FirstOrDefault(i => i.Reference.Equals(docRef)) is Document foundDocument)
                        {
                            parsedDocument = foundDocument;
                            parsedModel = foundDocument.GetModel();
                        }
                    }

                    if (ModelBuilderHelpers.Parse(App, parsedDocumentReference, parsedDocument?.Type, parsedModel, parsedDocument, foundPropertyDocument.EnumerateObject(), jsonSerializerOptions) is Document found)
                    {
                        foundDocuments.Add(new DocumentTimestamp(found, readTime, true));
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
                TransactionUsed != null &&
                doc.TryGetProperty("transaction", out JsonElement transactionElement) &&
                transactionElement.GetString() is string transactionToken)
            {
                TransactionUsed.Token = transactionToken;
            }
        }

        Document? lastDoc = foundDocuments.LastOrDefault()?.Document;

        response.Append(new QueryDocumentResult(
            foundDocuments.AsReadOnly(),
            skippedResults,
            skippedReadTime,
            page,
            query.SizeOfPages,
            response,
            (pageNum, ct) =>
            {
                return QueryDocumentPage(
                    response,
                    BuildStructureQuery(query, lastDoc, jsonSerializerOptions, ct),
                    pageNum,
                    0,
                    jsonSerializerOptions,
                    ct);
            }));

        return response;
    }

    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    internal async Task<HttpResponse<QueryDocumentResult<T>>> QueryDocumentPage<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>(
        HttpResponse<QueryDocumentResult<T>> response,
        Task<HttpResponse<StructuredQuery>> queryTask,
        int page,
        int offset,
        JsonSerializerOptions jsonSerializerOptions,
        CancellationToken cancellationToken)
        where T : class
    {
        var queryResponse = await queryTask;
        response.Append(queryResponse);
        if (queryResponse.IsError)
        {
            return response;
        }

        var query = queryResponse.Result;

        var (jsonDocument, queryDocumentResponse) = await ExecuteQueryDocument(
            page,
            offset,
            query,
            jsonSerializerOptions,
            cancellationToken);
        response.Append(queryDocumentResponse);
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

                        if (CacheDocuments.FirstOrDefault(i => i.Reference.Equals(docRef)) is Document<T> foundDocument)
                        {
                            parsedDocument = foundDocument;
                            parsedModel = foundDocument.Model;
                        }
                    }

                    if (ModelBuilderHelpers.Parse<T>(App, parsedDocumentReference, parsedModel, parsedDocument, foundPropertyDocument.EnumerateObject(), jsonSerializerOptions) is Document<T> found)
                    {
                        foundDocuments.Add(new DocumentTimestamp<T>(found, readTime, true));
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
                TransactionUsed != null &&
                doc.TryGetProperty("transaction", out JsonElement transactionElement) &&
                transactionElement.GetString() is string transactionToken)
            {
                TransactionUsed.Token = transactionToken;
            }
        }

        Document? lastDoc = foundDocuments.LastOrDefault()?.Document;

        response.Append(new QueryDocumentResult<T>(
            foundDocuments.AsReadOnly(),
            skippedResults,
            skippedReadTime,
            page,
            query.SizeOfPages,
            response,
            (pageNum, ct) =>
            {
                return QueryDocumentPage(
                    response,
                    BuildStructureQuery(query, lastDoc, jsonSerializerOptions, ct),
                    pageNum,
                    0,
                    jsonSerializerOptions,
                    ct);
            }));

        return response;
    }
}
