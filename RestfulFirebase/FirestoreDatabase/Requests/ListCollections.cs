using System;
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

namespace RestfulFirebase.FirestoreDatabase.Requests;

/// <summary>
/// Request to list the <see cref="Document{T}"/> of the specified request query.
/// </summary>
public class ListCollectionsRequest : FirestoreDatabaseRequest<TransactionResponse<ListCollectionsRequest, ListCollectionsResult>>
{
    /// <summary>
    /// Gets or sets the <see cref="JsonSerializerOptions"/> used to serialize and deserialize documents.
    /// </summary>
    public JsonSerializerOptions? JsonSerializerOptions { get; set; }

    /// <summary>
    /// Gets or sets the requested <see cref="References.DocumentReference"/> of the document node.
    /// </summary>
    public DocumentReference? DocumentReference { get; set; }

    /// <summary>
    /// Gets or sets the requested page size of the result <see cref="AsyncPager{T}"/>.
    /// </summary>
    public int? PageSize { get; set; }

    /// <inheritdoc cref="ListCollectionsRequest"/>
    /// <returns>
    /// The <see cref="Task"/> proxy that represents the <see cref="TransactionResponse"/> with the result <see cref="ListCollectionsResult"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <see cref="TransactionRequest.Config"/> is a null reference.
    /// </exception>
    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    internal override async Task<TransactionResponse<ListCollectionsRequest, ListCollectionsResult>> Execute()
    {
        ArgumentNullException.ThrowIfNull(Config);

        JsonSerializerOptions jsonSerializerOptions = ConfigureJsonSerializerOption(JsonSerializerOptions);

        try
        {
            var iterator = await ExecuteNextPage(null, jsonSerializerOptions);

            Func<CancellationToken, ValueTask<AsyncPager<CollectionReference>.DocumentPagerIterator>>? firstIterationIterator = null;
            if (iterator.NextPage != null)
            {
                firstIterationIterator = new Func<CancellationToken, ValueTask<AsyncPager<CollectionReference>.DocumentPagerIterator>>(
                    async (ct) => await iterator.NextPage!(ct));
            }
            var firstIteration = new ValueTask<AsyncPager<CollectionReference>.DocumentPagerIterator>(
                new AsyncPager<CollectionReference>.DocumentPagerIterator(iterator.Item, firstIterationIterator));
            AsyncPager<CollectionReference> pager = new(new(null!, (_) => firstIteration));
            ListCollectionsResult result = new(iterator.Item, pager);

            return new TransactionResponse<ListCollectionsRequest, ListCollectionsResult>(this, result, null);
        }
        catch (Exception ex)
        {
            return new TransactionResponse<ListCollectionsRequest, ListCollectionsResult>(this, null, ex);
        }
    }

    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    private async Task<AsyncPager<CollectionReference>.DocumentPagerIterator> ExecuteNextPage(string? pageToken, JsonSerializerOptions jsonSerializerOptions)
    {
        ArgumentNullException.ThrowIfNull(Config);

        string url;
        if (DocumentReference == null)
        {
            url =
                $"{Api.FirestoreDatabase.FirestoreDatabaseV1Endpoint}/" +
                $"{string.Format(Api.FirestoreDatabase.FirestoreDatabaseDocumentsEndpoint, Config.ProjectId, ":listCollectionIds")}";
        }
        else
        {
            url = DocumentReference.BuildUrl(Config.ProjectId, ":listCollectionIds");
        }

        using MemoryStream stream = new();
        Utf8JsonWriter writer = new(stream);

        writer.WriteStartObject();
        if (PageSize.HasValue)
        {
            writer.WritePropertyName("pageSize");
            writer.WriteNumberValue(PageSize.Value);
        }
        if (pageToken != null)
        {
            writer.WritePropertyName("pageToken");
            writer.WriteStringValue(pageToken);
        }
        writer.WriteEndObject();

        var response = await ExecuteWithContent(stream, HttpMethod.Post, url);
        using Stream contentStream = await response.Content.ReadAsStreamAsync();
        JsonDocument jsonDocument = await JsonDocument.ParseAsync(contentStream);

        List<CollectionReference> collectionReferences = new();
        string? nextPageToken = null;

        if (jsonDocument.RootElement.TryGetProperty("collectionIds", out JsonElement documentsProperty))
        {
            foreach (var doc in documentsProperty.EnumerateArray())
            {
                CollectionReference? collectionReference = ParseCollectionReference(doc, jsonSerializerOptions);
                if (collectionReference != null)
                {
                    collectionReferences.Add(collectionReference);
                }
            }
        }

        if (jsonDocument.RootElement.TryGetProperty("nextPageToken", out JsonElement nextPageTokenProperty))
        {
            nextPageToken = nextPageTokenProperty.Deserialize<string>(jsonSerializerOptions);
        }

        if (nextPageToken == null)
        {
            return new(collectionReferences.ToArray(), null);
        }
        else
        {
            return new(collectionReferences.ToArray(), async (ct) => await ExecuteNextPage(nextPageToken, jsonSerializerOptions));
        }
    }
}

/// <summary>
/// The result of the <see cref="ListCollectionsRequest"/> request.
/// </summary>
public class ListCollectionsResult
{
    /// <summary>
    /// Gets the first result of the list.
    /// </summary>
    public CollectionReference[] FirstResult { get; }

    /// <summary>
    /// Gets the pager iterator <see cref="AsyncPager{T}"/> to iterate to next page result.
    /// </summary>
    public AsyncPager<CollectionReference> CollectionPager { get; }

    internal ListCollectionsResult(CollectionReference[] firstResult, AsyncPager<CollectionReference> collectionPager)
    {
        FirstResult = firstResult;
        CollectionPager = collectionPager;
    }
}