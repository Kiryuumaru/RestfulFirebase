using RestfulFirebase.Common.Abstractions;
using RestfulFirebase.Common.Http;
using RestfulFirebase.FirestoreDatabase.Transactions;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace RestfulFirebase.FirestoreDatabase;

public partial class FirestoreDatabaseApi
{
    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
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
        using Stream contentStream = await lastHttpTransaction.ResponseMessage.Content.ReadAsStreamAsync(cancellationToken);
#else
        using Stream contentStream = await lastHttpTransaction.ResponseMessage.Content.ReadAsStreamAsync();
#endif
        JsonDocument jsonDocument = await JsonDocument.ParseAsync(contentStream, cancellationToken: cancellationToken);

        if (jsonDocument.RootElement.TryGetProperty("transaction", out JsonElement transactionElement) &&
            transactionElement.GetString() is string transactionToken)
        {
            transaction.Token = transactionToken;
        }

        return response.Append(transaction);
    }
}
