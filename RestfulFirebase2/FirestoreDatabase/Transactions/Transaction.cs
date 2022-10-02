using System;

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
        /// Gets the <see cref="Transactions.Transaction"/> of the builder. Has implicit conversion from
        /// <para><see cref="Transactions.Transaction"/></para>
        /// </summary>
        public Transaction Transaction { get; }

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
    /// Adds new instance of <see cref="ReadOnlyTransaction"/> to the builder.
    /// </summary>
    /// <param name="readTime">
    /// Reads documents at the given time. This may not be older than 60 seconds.
    /// </param>
    /// <returns>
    /// The <see cref="Builder"/>.
    /// </returns>
    public static Builder ReadOnly(DateTimeOffset? readTime = null)
    {
        return new Builder(new ReadOnlyTransaction(readTime));
    }

    /// <summary>
    /// Adds new instance of <see cref="ReadWriteTransaction"/> to the builder.
    /// </summary>
    /// <param name="retryTransaction">
    /// An optional transaction to retry.
    /// </param>
    /// <returns>
    /// The <see cref="Builder"/>.
    /// </returns>
    public static Builder ReadWrite(string? retryTransaction = null)
    {
        return new Builder(new ReadWriteTransaction(retryTransaction));
    }

    /// <summary>
    /// Gets the token for firebase database atomic operation.
    /// </summary>
    public string? Token { get; internal set; }
}
