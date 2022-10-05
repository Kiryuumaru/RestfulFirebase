namespace RestfulFirebase.FirestoreDatabase.Transactions;

/// <summary>
/// The transaction can be used for both read and write operations.
/// </summary>
public class ReadWriteTransaction : Transaction
{
    /// <summary>
    /// Gets an optional transaction to retry.
    /// </summary>
    public string? RetryTransaction { get; }

    internal ReadWriteTransaction(string? retryTransaction)
    {
        RetryTransaction = retryTransaction;
    }
}