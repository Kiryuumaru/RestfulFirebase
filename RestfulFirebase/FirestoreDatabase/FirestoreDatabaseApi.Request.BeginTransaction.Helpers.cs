using RestfulFirebase.Common.Abstractions;
using RestfulFirebase.FirestoreDatabase.Transactions;
using RestfulHelpers.Common;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace RestfulFirebase.FirestoreDatabase;

public partial class FirestoreDatabaseApi
{
    internal async Task<HttpResponse<TTransaction>> ExecuteBeginTransaction<TTransaction>(TTransaction transaction, IAuthorization? authorization, CancellationToken cancellationToken)
        where TTransaction : Transaction
    {
        string url = $"{FirestoreDatabaseV1Endpoint}/" +
            $"{string.Format(FirestoreDatabaseDocumentsEndpoint, App.Config.ProjectId, ":beginTransaction")}";

        using MemoryStream stream = new();
        Utf8JsonWriter writer = new(stream);

        writer.WriteStartObject();
        writer.WritePropertyName("options");
        BuildTransactionOption(writer, transaction);
        writer.WriteEndObject();

        await writer.FlushAsync(cancellationToken);

        HttpResponse<TTransaction> response = new();

        var postResponse = await ExecutePost(authorization, stream, url, cancellationToken);
        response.Append(postResponse);
        if (response.IsError || response.HttpTransactions.LastOrDefault() is not HttpTransaction lastHttpTransaction)
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

        if (jsonDocument.RootElement.TryGetProperty("transaction", out JsonElement transactionElement) &&
            transactionElement.GetString() is string transactionToken)
        {
            transaction.Token = transactionToken;
        }

        response.Append(transaction);

        return response;
    }
}
