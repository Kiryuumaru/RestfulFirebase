using RestfulFirebase.Common.Abstractions;
using RestfulFirebase.FirestoreDatabase.References;
using RestfulFirebase.FirestoreDatabase.Transactions;
using RestfulHelpers.Common;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace RestfulFirebase.FirestoreDatabase.Fetches;

public abstract partial class Fetch
{
    internal async Task<(JsonDocument?, HttpResponse)> ExecuteGetDocument(
        IEnumerable<DocumentReference> documentReferences,
        Transaction? transaction,
        IAuthorization? authorization,
        CancellationToken cancellationToken)
    {
        string url =
            $"{FirestoreDatabaseApi.FirestoreDatabaseV1Endpoint}/" +
            $"{string.Format(FirestoreDatabaseApi.FirestoreDatabaseDocumentsEndpoint, App.Config.ProjectId, ":batchGet")}";

        using MemoryStream stream = new();
        Utf8JsonWriter writer = new(stream);

        writer.WriteStartObject();
        writer.WritePropertyName("documents");
        writer.WriteStartArray();
        foreach (var documentReference in documentReferences)
        {
            writer.WriteStringValue(documentReference.BuildUrlCascade(App.Config.ProjectId));
        }
        writer.WriteEndArray();
        FirestoreDatabaseApi.BuildTransaction(writer, transaction, true);
        writer.WriteEndObject();

        await writer.FlushAsync(cancellationToken);

        var response = await App.FirestoreDatabase.ExecutePost(authorization, stream, url, cancellationToken);
        if (response.IsError || response.HttpTransactions.LastOrDefault() is not HttpTransaction lastHttpTransaction)
        {
            return (null, response);
        }

#if NET6_0_OR_GREATER
        using Stream? contentStream = lastHttpTransaction.ResponseMessage == null ? null : await lastHttpTransaction.ResponseMessage.Content.ReadAsStreamAsync(cancellationToken);
#else
        using Stream? contentStream = lastHttpTransaction.ResponseMessage == null ? null : await lastHttpTransaction.ResponseMessage.Content.ReadAsStreamAsync();
#endif
        return contentStream == null ? (null, response) : (await JsonDocument.ParseAsync(contentStream, cancellationToken: cancellationToken), response);
    }
}
