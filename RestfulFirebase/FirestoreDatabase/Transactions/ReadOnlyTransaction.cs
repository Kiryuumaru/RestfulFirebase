﻿using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace RestfulFirebase.FirestoreDatabase.Transactions;

/// <summary>
/// The transaction that can only be used for read operations.
/// </summary>
public class ReadOnlyTransaction : Transaction
{
    /// <summary>
    /// Creates the transaction that can only be used for read operations.
    /// </summary>
    /// <param name="readTime">
    /// Reads documents at the given time. This may not be older than 60 seconds.
    /// </param>
    public static ReadOnlyTransaction Create(DateTimeOffset? readTime = null)
    {
        return new(readTime); 
    }

    /// <summary>
    /// Gets the given time documents will read. This may not be older than 60 seconds.
    /// </summary>
    public DateTimeOffset? ReadTime { get; }

    internal ReadOnlyTransaction(DateTimeOffset? readTime)
    {
        ReadTime = readTime;
    }

    [RequiresUnreferencedCode("Calls System.Text.Json.JsonSerializer.Serialize<TValue>(TValue, JsonSerializerOptions)")]
    internal override void BuildUtf8JsonWriter(Utf8JsonWriter writer, FirebaseConfig config, JsonSerializerOptions? jsonSerializerOptions)
    {
        writer.WriteStartObject();
        writer.WritePropertyName("readOnly");
        writer.WriteStartObject();
        if (ReadTime.HasValue)
        {
            writer.WritePropertyName("readTime");
            writer.WriteStringValue(ReadTime.Value.ToUniversalTime());
        }
        writer.WriteEndObject();
        writer.WriteEndObject();
    }
}
