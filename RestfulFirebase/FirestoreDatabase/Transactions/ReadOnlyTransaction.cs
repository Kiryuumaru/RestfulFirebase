using System;

namespace RestfulFirebase.FirestoreDatabase.Transactions;

/// <summary>
/// The transaction that can only be used for read operations.
/// </summary>
public class ReadOnlyTransaction : Transaction
{
    /// <summary>
    /// Gets the given time documents will read. This may not be older than 60 seconds.
    /// </summary>
    public DateTimeOffset? ReadTime { get; }

    internal ReadOnlyTransaction(DateTimeOffset? readTime)
    {
        ReadTime = readTime;
    }
}
