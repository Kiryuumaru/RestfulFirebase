﻿using System;
using System.Collections.Generic;
using System.Text.Json;
using RestfulFirebase.FirestoreDatabase.References;
using RestfulFirebase.Common.Requests;
using System.Threading.Tasks;
using System.IO;
using System.Net.Http;
using System.Diagnostics.CodeAnalysis;
using RestfulFirebase.FirestoreDatabase.Models;
using System.Threading;
using RestfulFirebase.Common.Utilities;
using System.Linq;
using RestfulFirebase.FirestoreDatabase.Queries;
using RestfulFirebase.FirestoreDatabase.Transactions;
using System.Data;
using System.Reflection;
using RestfulFirebase.Common.Attributes;
using RestfulFirebase.FirestoreDatabase.Enums;

namespace RestfulFirebase.FirestoreDatabase.Requests;

/// <summary>
/// Request to list the <see cref="Document{T}"/> of the specified request query.
/// </summary>
/// <typeparam name="T">
/// The type of the model to populate the document fields.
/// </typeparam>
public class ListDocumentsRequest<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T> : FirestoreDatabaseRequest<TransactionResponse<ListDocumentsRequest<T>, ListDocumentsResult<T>>>
    where T : class
{
    /// <summary>
    /// Gets or sets the <see cref="System.Text.Json.JsonSerializerOptions"/> used to serialize and deserialize documents.
    /// </summary>
    public JsonSerializerOptions? JsonSerializerOptions { get; set; }

    /// <summary>
    /// Gets or sets the requested <see cref="References.CollectionReference"/> of the collection node.
    /// </summary>
    public CollectionReference? CollectionReference { get; set; }

    /// <summary>
    /// Gets or sets the cache <see cref="Document{T}"/> documents to get and patch.
    /// </summary>
    public Document<T>.Builder? Document { get; set; }

    /// <summary>
    /// Gets or sets the requested page size of the result <see cref="AsyncPager{T}"/>.
    /// </summary>
    public int? PageSize { get; set; }

    /// <summary>
    /// Gets or sets <c>true</c> if the list should show missing documents; otherwise, <c>false</c>.
    /// </summary>
    public bool? ShowMissing { get; set; }

    /// <summary>
    /// Gets or sets the order to sort results by.
    /// </summary>
    public OrderByQuery.Builder? OrderBy { get; set; }

    /// <summary>
    /// Gets or sets the <see cref="Transactions.Transaction"/> for atomic operation.
    /// </summary>
    public Transaction? Transaction { get; set; }

    /// <inheritdoc cref="ListDocumentsRequest{T}"/>
    /// <returns>
    /// The <see cref="Task"/> proxy that represents the <see cref="TransactionResponse"/> with the result <see cref="ListDocumentsResult{T}"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <see cref="TransactionRequest.Config"/> or
    /// <see cref="CollectionReference"/> is a null reference.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <see cref="OrderBy"/> has parameter that does not exists in the model as firebase value.
    /// </exception>
    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    internal override async Task<TransactionResponse<ListDocumentsRequest<T>, ListDocumentsResult<T>>> Execute()
    {
        ArgumentNullException.ThrowIfNull(Config);
        ArgumentNullException.ThrowIfNull(CollectionReference);

        JsonSerializerOptions jsonSerializerOptions = ConfigureJsonSerializerOption(JsonSerializerOptions);

        string? orderBy = null;
        if (OrderBy != null && OrderBy.OrderByQuery.Any())
        {
            Type objType = typeof(T);

            PropertyInfo[] propertyInfos = objType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            FieldInfo[] fieldInfos = objType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            bool includeOnlyWithAttribute = objType.GetCustomAttribute(typeof(FirebaseValueOnlyAttribute)) != null;

            List<string> orderByQuery = new();

            foreach (var order in OrderBy.OrderByQuery)
            {
                var documentField = ClassMemberHelpers.GetDocumentField(propertyInfos, fieldInfos, includeOnlyWithAttribute, null, order.PropertyName, jsonSerializerOptions);

                if (documentField == null)
                {
                    throw new ArgumentException($"\"{order.PropertyName}\" does not exist in the model \"{objType.Name}\".");
                }

                orderByQuery.Add($"{documentField.DocumentFieldName} {(order.OrderDirection == OrderDirection.Ascending ? "asc" : "desc")}");
            }

            orderBy = string.Join(",", orderByQuery);
        }

        AsyncPager<Document<T>>.DocumentPagerIterator iterator;
        try
        {
            iterator = await ExecuteNextPage(null, orderBy, jsonSerializerOptions);
        }
        catch (Exception ex)
        {
            return new TransactionResponse<ListDocumentsRequest<T>, ListDocumentsResult<T>>(this, null, ex);
        }

        Func<CancellationToken, ValueTask<AsyncPager<Document<T>>.DocumentPagerIterator>>? firstIterationIterator = null;
        if (iterator.NextPage != null)
        {
            firstIterationIterator = new Func<CancellationToken, ValueTask<AsyncPager<Document<T>>.DocumentPagerIterator>>(
                async (ct) => await iterator.NextPage!(ct));
        }
        var firstIteration = new ValueTask<AsyncPager<Document<T>>.DocumentPagerIterator>(
            new AsyncPager<Document<T>>.DocumentPagerIterator(iterator.Item, firstIterationIterator));
        AsyncPager<Document<T>> pager = new(new(null!, (_) => firstIteration));
        ListDocumentsResult<T> result = new(iterator.Item, pager);

        return new TransactionResponse<ListDocumentsRequest<T>, ListDocumentsResult<T>>(this, result, null);
    }

    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    private async Task<AsyncPager<Document<T>>.DocumentPagerIterator> ExecuteNextPage(string? pageToken, string? orderBy, JsonSerializerOptions jsonSerializerOptions)
    {
        ArgumentNullException.ThrowIfNull(Config);
        ArgumentNullException.ThrowIfNull(CollectionReference);

        QueryBuilder qb = new();
        if (pageToken != null)
        {
            qb.Add("pageToken", pageToken);
        }
        if (PageSize.HasValue)
        {
            qb.Add("pageSize", PageSize.Value.ToString());
        }
        if (ShowMissing.HasValue)
        {
            qb.Add("showMissing", ShowMissing.Value ? "true" : "false");
        }
        if (orderBy != null)
        {
            qb.Add("orderBy", orderBy);
        }
        if (Transaction?.Token != null)
        {
            qb.Add("transaction", Transaction.Token);
        }
        string url = CollectionReference.BuildUrl(Config.ProjectId, qb.Build());

        var (executeResult, executeException) = await Execute(HttpMethod.Get, url);
        if (executeResult == null)
        {
            throw executeException ?? new Exception("Unknown exception occured");
        }

        using Stream contentStream = await executeResult.Content.ReadAsStreamAsync();
        JsonDocument jsonDocument = await JsonDocument.ParseAsync(contentStream);

        List<Document<T>> documents = new();
        string? nextPageToken = null;
        if (jsonDocument.RootElement.TryGetProperty("documents", out JsonElement documentsProperty))
        {
            foreach (var doc in documentsProperty.EnumerateArray())
            {
                DocumentReference? documentReference = null;
                Document<T>? document = null;
                T? model = null;
                if (doc.TryGetProperty("name", out JsonElement foundNameProperty) &&
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

                if (Document<T>.Parse(documentReference, model, document, doc.EnumerateObject(), jsonSerializerOptions) is Document<T> found)
                {
                    documents.Add(found);
                }
            }
        }

        if (jsonDocument.RootElement.TryGetProperty("nextPageToken", out JsonElement nextPageTokenProperty))
        {
            nextPageToken = nextPageTokenProperty.Deserialize<string>(jsonSerializerOptions);
        }

        if (nextPageToken == null)
        {
            return new(documents.ToArray(), null);
        }
        else
        {
            return new(documents.ToArray(), async (ct) => await ExecuteNextPage(nextPageToken, orderBy, jsonSerializerOptions));
        }
    }
}

/// <summary>
/// The result of the <see cref="ListDocumentsRequest{T}"/> request.
/// </summary>
/// <typeparam name="T">
/// The type of the model to populate the document fields.
/// </typeparam>
public class ListDocumentsResult<T>
    where T : class
{
    /// <summary>
    /// Gets the first result of the list.
    /// </summary>
    public Document<T>[] FirstResult { get; }

    /// <summary>
    /// Gets the pager iterator <see cref="AsyncPager{T}"/> to iterate to next page result.
    /// </summary>
    public AsyncPager<Document<T>> DocumentPager { get; }

    internal ListDocumentsResult(Document<T>[] firstResult, AsyncPager<Document<T>> documentPager)
    {
        FirstResult = firstResult;
        DocumentPager = documentPager;
    }
}