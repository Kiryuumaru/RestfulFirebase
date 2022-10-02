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
using System.Reflection;

namespace RestfulFirebase.FirestoreDatabase.Requests;

/// <summary>
/// Request to list the <see cref="Document{T}"/> of the specified request query.
/// </summary>
public class ListCollectionsRequest : FirestoreDatabaseRequest<TransactionResponse<ListCollectionsRequest, ListCollectionsResult>>
{
    /// <summary>
    /// Gets or sets the <see cref="System.Text.Json.JsonSerializerOptions"/> used to serialize and deserialize documents.
    /// </summary>
    public JsonSerializerOptions? JsonSerializerOptions { get; set; }

    /// <summary>
    /// Gets or sets the requested <see cref="References.DocumentReference"/> of the document node.
    /// </summary>
    public DocumentReference? DocumentReference { get; set; }

    /// <summary>
    /// Gets or sets the requested page size of the pager <see cref="ListCollectionsResult.GetAsyncEnumerator(CancellationToken)"/>.
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

        var firstResponse = await ExecuteNextPage(null, Config, jsonSerializerOptions, default);
        if (firstResponse.IsError)
        {
            return new(this, null, firstResponse.Error);
        }

        return new(this, firstResponse.Result, null);
    }

    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    private async Task<TransactionResponse<ListCollectionsRequest, ListCollectionsResult>> ExecuteNextPage(
        string? pageToken,
        FirebaseConfig config,
        JsonSerializerOptions jsonSerializerOptions,
        CancellationToken cancellationToken)
    {
        CancellationToken linkedCancellationToken = CancellationTokenSource.CreateLinkedTokenSource(CancellationToken, cancellationToken).Token;

        string url;
        if (DocumentReference == null)
        {
            url =
                $"{Api.FirestoreDatabase.FirestoreDatabaseV1Endpoint}/" +
                $"{string.Format(Api.FirestoreDatabase.FirestoreDatabaseDocumentsEndpoint, config.ProjectId, ":listCollectionIds")}";
        }
        else
        {
            url = DocumentReference.BuildUrl(config.ProjectId, ":listCollectionIds");
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

        var (executeResult, executeException) = await ExecuteWithContent(stream, HttpMethod.Post, url);
        if (executeResult == null)
        {
            throw executeException ?? new Exception("Unknown exception occured");
        }

#if NET6_0_OR_GREATER
        using Stream contentStream = await executeResult.Content.ReadAsStreamAsync(linkedCancellationToken);
#else
        using Stream contentStream = await executeResult.Content.ReadAsStreamAsync();
#endif
        JsonDocument jsonDocument = await JsonDocument.ParseAsync(contentStream, cancellationToken: linkedCancellationToken);

        List<CollectionReference> collectionReferences = new();
        string? nextPageToken = null;

        if (jsonDocument.RootElement.TryGetProperty("collectionIds", out JsonElement documentsProperty))
        {
            foreach (var doc in documentsProperty.EnumerateArray())
            {
                CollectionReference? collectionReference = CollectionReference.Parse(doc, jsonSerializerOptions);
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

        TransactionResponse<ListCollectionsRequest, ListCollectionsResult> response = null!;
        response = new(
            this,
            new(collectionReferences.ToArray(),
                nextPageToken,
                () => response,
                (nextPageTok, ct) =>
                {
                    return ExecuteNextPage(nextPageTok, config, jsonSerializerOptions, ct);
                }),
            null);
        return response;
    }
}

/// <summary>
/// The result of the <see cref="ListCollectionsRequest"/> request.
/// </summary>
public class ListCollectionsResult : IAsyncEnumerable<TransactionResponse<ListCollectionsRequest, ListCollectionsResult>>
{
    /// <summary>
    /// Gets the first result of the list.
    /// </summary>
    public CollectionReference[] CollectionReferences { get; }

    private readonly string? nextPageToken;
    private readonly Func<TransactionResponse<ListCollectionsRequest, ListCollectionsResult>> firstResponseFactory;
    private readonly Func<string, CancellationToken, Task<TransactionResponse<ListCollectionsRequest, ListCollectionsResult>>> pager;

    internal ListCollectionsResult(
        CollectionReference[] collectionReferences,
        string? nextPageToken,
        Func<TransactionResponse<ListCollectionsRequest, ListCollectionsResult>> firstResponseFactory,
        Func<string, CancellationToken, Task<TransactionResponse<ListCollectionsRequest, ListCollectionsResult>>> pager)
    {
        CollectionReferences = collectionReferences;
        this.nextPageToken = nextPageToken;
        this.firstResponseFactory = firstResponseFactory;
        this.pager = pager;
    }

    /// <inheritdoc/>
    public IAsyncEnumerator<TransactionResponse<ListCollectionsRequest, ListCollectionsResult>> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
        return new AsyncEnumerator(nextPageToken, firstResponseFactory(), CancellationTokenSource.CreateLinkedTokenSource(cancellationToken));
    }

    internal class AsyncEnumerator : IAsyncEnumerator<TransactionResponse<ListCollectionsRequest, ListCollectionsResult>>
    {
        public TransactionResponse<ListCollectionsRequest, ListCollectionsResult> Current { get; private set; } = default!;

        private ListCollectionsResult lastSuccessResult;

        private readonly string? nextPageToken;
        private readonly TransactionResponse<ListCollectionsRequest, ListCollectionsResult> firstResponse;
        private readonly CancellationTokenSource cancellationTokenSource;

        public AsyncEnumerator(string? nextPageToken, TransactionResponse<ListCollectionsRequest, ListCollectionsResult> firstResponse, CancellationTokenSource cancellationTokenSource)
        {
            firstResponse.ThrowIfError();
            this.nextPageToken = nextPageToken;
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
                if (nextPageToken == null)
                {
                    return false;
                }
                else
                {
                    Current = await lastSuccessResult.pager.Invoke(nextPageToken, cancellationTokenSource.Token);
                    if (Current.IsSuccess)
                    {
                        lastSuccessResult = Current.Result;
                    }
                    return true;
                }
            }
        }
    }
}