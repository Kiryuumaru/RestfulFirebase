using System;

namespace RestfulFirebase.FirestoreDatabase.Transactions;

/// <summary>
/// The options for creating a new transaction.
/// </summary>
public abstract class TransactionOption
{
    /// <summary>
    /// The transaction can only be used for read operations.
    /// </summary>
    /// <param name="readTime">
    /// Reads documents at the given time. This may not be older than 60 seconds.
    /// </param>
    public static ReadOnlyOption ReadOnly(DateTimeOffset? readTime = default)
    {
        return new(readTime);
    }

    /// <summary>
    /// The transaction can be used for both read and write operations.
    /// </summary>
    /// <param name="retryTransaction">
    /// An optional transaction to retry.
    /// </param>
    public static ReadWriteOption ReadWrite(string? retryTransaction = default)
    {
        return new(retryTransaction);
    }
}

/// <summary>
/// The transaction can only be used for read operations.
/// </summary>
public class ReadOnlyOption : TransactionOption
{
    /// <summary>
    /// Gets the given time documents will read. This may not be older than 60 seconds.
    /// </summary>
    public DateTimeOffset? ReadTime { get; }

    internal ReadOnlyOption(DateTimeOffset? readTime)
    {
        ReadTime = readTime;
    }
}

/// <summary>
/// The transaction can be used for both read and write operations.
/// </summary>
public class ReadWriteOption : TransactionOption
{
    /// <summary>
    /// Gets an optional transaction to retry.
    /// </summary>
    public string? RetryTransaction { get; }

	internal ReadWriteOption(string? retryTransaction)
	{
        RetryTransaction = retryTransaction;

    }
}
