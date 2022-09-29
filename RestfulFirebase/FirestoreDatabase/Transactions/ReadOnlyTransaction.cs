using System;

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
}
