﻿using System;
using RestfulFirebase.Common.Requests;
using System.Threading.Tasks;
using System.Net.Http;
using System.IO;
using System.Text.Json;
using RestfulFirebase.FirestoreDatabase.Transactions;
using System.Diagnostics.CodeAnalysis;

namespace RestfulFirebase.FirestoreDatabase.Requests;

/// <summary>
/// Request to run a query.
/// </summary>
public class RunQueryRequest : FirestoreDatabaseRequest<TransactionResponse<RunQueryRequest, Transaction>>
{
    /// <summary>
    /// Gets or sets the <see cref="System.Text.Json.JsonSerializerOptions"/> used to serialize and deserialize documents.
    /// </summary>
    public JsonSerializerOptions? JsonSerializerOptions { get; set; }

    /// <summary>
    /// Gets or sets the <see cref="Transactions.Transaction"/> for atomic operation.
    /// </summary>
    public Transaction? Transaction { get; set; }

    /// <inheritdoc cref="RunQueryRequest"/>
    /// <returns>
    /// The <see cref="Task"/> proxy that represents the <see cref="TransactionResponse"/> with the result <see cref="Transaction"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <see cref="TransactionRequest.Config"/> or
    /// <see cref="Transaction"/> is a null reference.
    /// </exception>
    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    internal override async Task<TransactionResponse<RunQueryRequest, Transaction>> Execute()
    {
        ArgumentNullException.ThrowIfNull(Config);
        ArgumentNullException.ThrowIfNull(Transaction);

        JsonSerializerOptions jsonSerializerOptions = ConfigureJsonSerializerOption(JsonSerializerOptions);

        using MemoryStream stream = new();
        Utf8JsonWriter writer = new(stream);

        writer.WriteStartObject();
        writer.WritePropertyName("options");
        Transaction.Transaction.BuildUtf8JsonWriter(writer, Config, jsonSerializerOptions);
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
            $"{string.Format(Api.FirestoreDatabase.FirestoreDatabaseDocumentsEndpoint, Config.ProjectId, ":runQuery")}";
    }
}
