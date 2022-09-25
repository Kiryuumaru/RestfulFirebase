using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace RestfulFirebase.FirestoreDatabase.Transactions;

/// <summary>
/// The atomic operation token for firebase database.
/// </summary>
public abstract class Transaction
{
    /// <summary>
    /// The options for creating a new transaction.
    /// </summary>
    public class Builder
    {
        /// <summary>
        /// Gets the <see cref="Transactions.Transaction"/> of the builder.
        /// </summary>
        public Transaction Transaction { get; }

        /// <returns>
        /// The <see cref="Builder"/> with the created transaction.
        /// </returns>
        /// <inheritdoc cref="ReadOnlyTransaction.Create(DateTimeOffset?)"/>
        public static Builder ReadOnly(DateTimeOffset? readTime = null)
        {
            return new(ReadOnlyTransaction.Create(readTime));
        }

        /// <returns>
        /// The <see cref="Builder"/> with the created transaction.
        /// </returns>
        /// <inheritdoc cref="ReadWriteTransaction.Create(string?)"/>
        public static Builder ReadWrite(string? retryTransaction = null)
        {
            return new(ReadWriteTransaction.Create(retryTransaction));
        }

        internal Builder(Transaction transaction)
        {
            Transaction = transaction;
        }

        /// <summary>
        /// Converts the <see cref="Transactions.Transaction"/> to <see cref="Builder"/>
        /// </summary>
        /// <param name="transaction">
        /// The <see cref="Transactions.Transaction"/> to convert.
        /// </param>
        public static implicit operator Builder(Transaction transaction)
        {
            return new(transaction);
        }
    }

    /// <summary>
    /// Gets the token for firebase database atomic operation.
    /// </summary>
    public string? Token { get; internal set; }

    [RequiresUnreferencedCode("Calls System.Text.Json.JsonSerializer.Serialize<TValue>(TValue, JsonSerializerOptions)")]
    internal abstract void BuildUtf8JsonWriter(Utf8JsonWriter writer, FirebaseConfig config, JsonSerializerOptions? jsonSerializerOptions);
}