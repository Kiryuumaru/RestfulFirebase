using RestfulFirebase.Common.Requests;
using System.Threading.Tasks;
using System.Net.Http;
using System.IO;
using System.Text.Json;
using RestfulFirebase.FirestoreDatabase.Transactions;
using System.Diagnostics.CodeAnalysis;

namespace RestfulFirebase.FirestoreDatabase.Requests;

/// <summary>
/// Request a transaction to start an atomic operation.
/// </summary>
public class BeginTransactionRequest : FirestoreDatabaseRequest<TransactionResponse<BeginTransactionRequest, Transaction>>
{
    /// <summary>
    /// Gets or sets the <see cref="System.Text.Json.JsonSerializerOptions"/> used to serialize and deserialize documents.
    /// </summary>
    public JsonSerializerOptions? JsonSerializerOptions { get; set; }

    /// <summary>
    /// Gets or sets the <see cref="Transaction.Builder"/> of the transaction.
    /// </summary>
    public Transaction.Builder? Transaction { get; set; }

    /// <inheritdoc cref="BeginTransactionRequest"/>
    /// <returns>
    /// The <see cref="Task"/> proxy that represents the <see cref="TransactionResponse"/> with the result <see cref="Transaction"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <see cref="TransactionRequest.Config"/> or
    /// <see cref="Transaction"/> is a null reference.
    /// </exception>
    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    internal override async Task<TransactionResponse<BeginTransactionRequest, Transaction>> Execute()
    {
        ArgumentNullException.ThrowIfNull(Config);
        ArgumentNullException.ThrowIfNull(Transaction);

        JsonSerializerOptions jsonSerializerOptions = ConfigureJsonSerializerOption(JsonSerializerOptions);

        using MemoryStream stream = new();
        Utf8JsonWriter writer = new(stream);

        writer.WriteStartObject();
        writer.WritePropertyName("options");
        if (Transaction.Transaction is ReadOnlyTransaction readOnlyTransaction)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("readOnly");
            writer.WriteStartObject();
            if (readOnlyTransaction.ReadTime.HasValue)
            {
                writer.WritePropertyName("readTime");
                writer.WriteStringValue(readOnlyTransaction.ReadTime.Value.ToUniversalTime());
            }
            writer.WriteEndObject();
            writer.WriteEndObject();
        }
        else if (Transaction.Transaction is ReadWriteTransaction readWriteTransaction)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("readWrite");
            writer.WriteStartObject();
            if (readWriteTransaction.RetryTransaction != null)
            {
                writer.WritePropertyName("retryTransaction");
                writer.WriteStringValue(readWriteTransaction.RetryTransaction);
            }
            writer.WriteEndObject();
            writer.WriteEndObject();
        }
        writer.WriteEndObject();

        await writer.FlushAsync();

        var (executeResult, executeException) = await ExecuteWithContent(stream, HttpMethod.Post, BuildUrl());
        if (executeResult == null)
        {
            return new(this, null, executeException);
        }

        using Stream contentStream = await executeResult.Content.ReadAsStreamAsync();
        JsonDocument jsonDocument = await JsonDocument.ParseAsync(contentStream);

        if (jsonDocument.RootElement.TryGetProperty("transaction", out JsonElement transactionElement) &&
            transactionElement.GetString() is string transactionToken)
        {
            Transaction.Transaction.Token = transactionToken;
        }

        return new(this, Transaction.Transaction, null);
    }

    internal string BuildUrl()
    {
        ArgumentNullException.ThrowIfNull(Config);

        return
            $"{Api.FirestoreDatabase.FirestoreDatabaseV1Endpoint}/" +
            $"{string.Format(Api.FirestoreDatabase.FirestoreDatabaseDocumentsEndpoint, Config.ProjectId, ":beginTransaction")}";
    }
}
