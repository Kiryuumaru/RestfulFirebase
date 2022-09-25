using System;
using RestfulFirebase.Common.Requests;
using System.Threading.Tasks;
using System.Net.Http;
using System.IO;
using System.Text.Json;
using RestfulFirebase.FirestoreDatabase.Transactions;

namespace RestfulFirebase.FirestoreDatabase.Requests;

/// <summary>
/// Request a transaction to start an atomic operation.
/// </summary>
public class BeginTransactionRequest : FirestoreDatabaseRequest<TransactionResponse<BeginTransactionRequest, Transaction>>
{
    /// <summary>
    /// Gets or sets the <see cref="TransactionOption"/> of the transaction.
    /// </summary>
    public TransactionOption? Option { get; set; }

    /// <inheritdoc cref="BeginTransactionRequest"/>
    /// <returns>
    /// The <see cref="Task"/> proxy that represents the <see cref="TransactionResponse"/> with the result <see cref="Transaction"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <see cref="TransactionRequest.Config"/> or
    /// <see cref="Option"/> is a null reference.
    /// </exception>
    internal override async Task<TransactionResponse<BeginTransactionRequest, Transaction>> Execute()
    {
        ArgumentNullException.ThrowIfNull(Config);
        ArgumentNullException.ThrowIfNull(Option);

        using MemoryStream stream = new();
        Utf8JsonWriter writer = new(stream);

        writer.WriteStartObject();
        writer.WritePropertyName("options");
        writer.WriteStartObject();
        if (Option is ReadOnlyOption readOnlyOption)
        {
            writer.WritePropertyName("readOnly");
            writer.WriteStartObject();
            if (readOnlyOption.ReadTime.HasValue)
            {
                writer.WritePropertyName("readTime");
                writer.WriteStringValue(readOnlyOption.ReadTime.Value.ToUniversalTime());
            }
            writer.WriteEndObject();
        }
        if (Option is ReadWriteOption readWriteOption)
        {
            writer.WritePropertyName("readWrite");
            writer.WriteStartObject();
            if (readWriteOption.RetryTransaction != null)
            {
                writer.WritePropertyName("retryTransaction");
                writer.WriteStringValue(readWriteOption.RetryTransaction);
            }
            writer.WriteEndObject();
        }
        writer.WriteEndObject();
        writer.WriteEndObject();

        await writer.FlushAsync();

        var (executeResult, executeException) = await ExecuteWithContent(stream, HttpMethod.Post, BuildUrl());
        if (executeResult == null)
        {
            return new(this, null, executeException);
        }

        using Stream contentStream = await executeResult.Content.ReadAsStreamAsync();
        JsonDocument jsonDocument = await JsonDocument.ParseAsync(contentStream);

        Transaction? transaction = null;

        if (jsonDocument.RootElement.TryGetProperty("transaction", out JsonElement transactionElement) &&
            transactionElement.GetString() is string transactionToken)
        {
            transaction = new(transactionToken);
        }

        return new(this, transaction, null);
    }

    internal string BuildUrl()
    {
        ArgumentNullException.ThrowIfNull(Config);

        return
            $"{Api.FirestoreDatabase.FirestoreDatabaseV1Endpoint}/" +
            $"{string.Format(Api.FirestoreDatabase.FirestoreDatabaseDocumentsEndpoint, Config.ProjectId, ":beginTransaction")}";
    }
}
