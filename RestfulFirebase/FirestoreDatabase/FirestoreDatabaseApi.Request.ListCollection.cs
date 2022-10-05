using System;
using System.Collections.Generic;
using System.Text.Json;
using RestfulFirebase.FirestoreDatabase.References;
using System.Threading.Tasks;
using System.IO;
using System.Net.Http;
using System.Diagnostics.CodeAnalysis;
using RestfulFirebase.FirestoreDatabase.Models;
using System.Threading;
using System.Reflection;
using RestfulFirebase.Common.Http;
using RestfulFirebase.Common.Abstractions;
using System.Linq;

namespace RestfulFirebase.FirestoreDatabase;

public partial class FirestoreDatabaseApi
{
    /// <summary>
    /// Request to list the <see cref="Document{T}"/> of the specified request query.
    /// </summary>
    /// <param name="pageSize">
    /// The requested page size of the pager <see cref="ListCollectionResult.GetAsyncEnumerator(CancellationToken)"/>.
    /// </param>
    /// <param name="documentReference">
    /// The requested <see cref="DocumentReference"/> of the document node.
    /// </param>
    /// <param name="jsonSerializerOptions">
    /// The <see cref="JsonSerializerOptions"/> used to serialize and deserialize documents.
    /// </param>
    /// <param name="authorization">
    /// The authorization used for the operation.
    /// </param>
    /// <param name="cancellationToken">
    /// The <see cref="CancellationToken"/> that propagates notification if the operations should be canceled.
    /// </param>
    /// <returns>
    /// The <see cref="Task"/> proxy that represents the <see cref="HttpResponse"/> with the result <see cref="ListCollectionResult"/>.
    /// </returns>
    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    public async Task<HttpResponse<ListCollectionResult>> ListCollection(int? pageSize = null, DocumentReference? documentReference = default, IAuthorization? authorization = default, JsonSerializerOptions? jsonSerializerOptions = default, CancellationToken cancellationToken = default)
    {
        JsonSerializerOptions options = ConfigureJsonSerializerOption(jsonSerializerOptions);

        return await ExecuteListCollectionNextPage(new(), null, pageSize, documentReference, authorization, options, cancellationToken);
    }

    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    private async Task<HttpResponse<ListCollectionResult>> ExecuteListCollectionNextPage(
        HttpResponse<ListCollectionResult> response,
        string? pageToken,
        int? pageSize,
        DocumentReference? documentReference,
        IAuthorization? authorization,
        JsonSerializerOptions options,
        CancellationToken cancellationToken)
    {
        string url;
        if (documentReference == null)
        {
            url =
                $"{FirestoreDatabaseV1Endpoint}/" +
                $"{string.Format(FirestoreDatabaseDocumentsEndpoint, App.Config.ProjectId, ":listCollectionIds")}";
        }
        else
        {
            url = documentReference.BuildUrl(App.Config.ProjectId, ":listCollectionIds");
        }

        using MemoryStream stream = new();
        Utf8JsonWriter writer = new(stream);

        writer.WriteStartObject();
        if (pageSize.HasValue)
        {
            writer.WritePropertyName("pageSize");
            writer.WriteNumberValue(pageSize.Value);
        }
        if (pageToken != null)
        {
            writer.WritePropertyName("pageToken");
            writer.WriteStringValue(pageToken);
        }
        writer.WriteEndObject();

        var postResponse = await ExecutePost(authorization, stream, url, cancellationToken);
        response.Concat(postResponse);
        if (postResponse.IsError || postResponse.HttpTransactions.LastOrDefault() is not HttpTransaction lastHttpTransaction)
        {
            return response;
        }

#if NET6_0_OR_GREATER
        using Stream contentStream = await lastHttpTransaction.HttpResponseMessage.Content.ReadAsStreamAsync(cancellationToken);
#else
        using Stream contentStream = await lastHttpTransaction.HttpResponseMessage.Content.ReadAsStreamAsync();
#endif
        JsonDocument jsonDocument = await JsonDocument.ParseAsync(contentStream, cancellationToken: cancellationToken);

        List<CollectionReference> collectionReferences = new();
        string? nextPageToken = null;

        if (jsonDocument.RootElement.TryGetProperty("collectionIds", out JsonElement documentsProperty))
        {
            foreach (var doc in documentsProperty.EnumerateArray())
            {
                CollectionReference? collectionReference = CollectionReference.Parse(App, doc, options);
                if (collectionReference != null)
                {
                    collectionReferences.Add(collectionReference);
                }
            }
        }

        if (jsonDocument.RootElement.TryGetProperty("nextPageToken", out JsonElement nextPageTokenProperty))
        {
            nextPageToken = nextPageTokenProperty.Deserialize<string>(options);
        }

        return response.Concat(new ListCollectionResult(
            collectionReferences.ToArray(),
            nextPageToken,
            () => response,
            (nextPageTok, ct) =>
            {
                return ExecuteListCollectionNextPage(response, nextPageTok, pageSize, documentReference, authorization, options, ct);
            }));
    }
}
