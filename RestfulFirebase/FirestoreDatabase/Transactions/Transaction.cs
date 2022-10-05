using System;

namespace RestfulFirebase.FirestoreDatabase.Transactions;

/// <summary>
/// The atomic operation token for firebase database.
/// </summary>
public abstract class Transaction
{
    /// <summary>
    /// Gets the token for firebase database atomic operation.
    /// </summary>
    public string? Token { get; internal set; }
}
