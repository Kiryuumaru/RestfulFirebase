using System.Collections.Generic;
using System.Text.Json;
using RestfulFirebase.FirestoreDatabase.References;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics.CodeAnalysis;
using RestfulFirebase.FirestoreDatabase.Models;
using System.Threading;
using RestfulFirebase.Common.Abstractions;
using System.Linq;
using RestfulHelpers.Common;

namespace RestfulFirebase.FirestoreDatabase;

public partial class FirestoreDatabaseApi
{
    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    private async Task<HttpResponse<ListCollectionResult>> ExecuteListCollectionNextPage(
        HttpResponse<ListCollectionResult> response,
        string? pageToken,
        int? pageSize,
        DocumentReference? documentReference,
        IAuthorization? authorization,
        JsonSerializerOptions jsonSerializerOptions,
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
        response.Append(postResponse);
        if (postResponse.IsError || postResponse.HttpTransactions.LastOrDefault() is not HttpTransaction lastHttpTransaction)
        {
            return response;
        }

#if NET6_0_OR_GREATER
        using Stream? contentStream = lastHttpTransaction.ResponseMessage == null ? null : await lastHttpTransaction.ResponseMessage.Content.ReadAsStreamAsync(cancellationToken);
#else
        using Stream? contentStream = lastHttpTransaction.ResponseMessage == null ? null : await lastHttpTransaction.ResponseMessage.Content.ReadAsStreamAsync();
#endif
        if (contentStream == null)
        {
            return response;
        }

        JsonDocument jsonDocument = await JsonDocument.ParseAsync(contentStream, cancellationToken: cancellationToken);

        List<CollectionReference> collectionReferences = new();
        string? nextPageToken = null;

        if (jsonDocument.RootElement.TryGetProperty("collectionIds", out JsonElement documentsProperty))
        {
            foreach (var doc in documentsProperty.EnumerateArray())
            {
                CollectionReference? collectionReference = CollectionReference.Parse(App, doc, jsonSerializerOptions);
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

        response.Append(new ListCollectionResult(
            collectionReferences.AsReadOnly(),
            nextPageToken,
            response,
            (nextPageTok, ct) =>
            {
                return ExecuteListCollectionNextPage(response, nextPageTok, pageSize, documentReference, authorization, jsonSerializerOptions, ct);
            }));

        return response;
    }
}
