﻿using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace RestfulFirebase.FirestoreDatabase.Transactions;

/// <summary>
/// The transaction can be used for both read and write operations.
/// </summary>
public class ReadWriteTransaction : Transaction
{
    /// <summary>
    /// Creates the transaction that can be used for both read and write operations.
    /// </summary>
    /// <param name="retryTransaction">
    /// An optional transaction to retry.
    /// </param>
    public static ReadWriteTransaction Create(string? retryTransaction = null)
    {
        return new(retryTransaction);
    }

    /// <summary>
    /// Gets an optional transaction to retry.
    /// </summary>
    public string? RetryTransaction { get; }

    internal ReadWriteTransaction(string? retryTransaction)
    {
        RetryTransaction = retryTransaction;
    }

    [RequiresUnreferencedCode("Calls System.Text.Json.JsonSerializer.Serialize<TValue>(TValue, JsonSerializerOptions)")]
    internal override void BuildUtf8JsonWriter(Utf8JsonWriter writer, FirebaseConfig config, JsonSerializerOptions? jsonSerializerOptions)
    {
        writer.WriteStartObject();
        writer.WritePropertyName("readWrite");
        writer.WriteStartObject();
        if (RetryTransaction != null)
        {
            writer.WritePropertyName("retryTransaction");
            writer.WriteStringValue(RetryTransaction);
        }
        writer.WriteEndObject();
        writer.WriteEndObject();
    }
}