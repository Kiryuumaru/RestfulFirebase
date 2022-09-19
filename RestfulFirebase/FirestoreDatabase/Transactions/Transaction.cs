using System;
using System.Collections.Generic;
using System.Text;

namespace RestfulFirebase.FirestoreDatabase.Transactions;

/// <summary>
/// The atomic operation token for firebase database.
/// </summary>
public class Transaction
{
    /// <summary>
    /// Gets the token for firebase database atomic operation.
    /// </summary>
    public string Token { get; }

	internal Transaction(string token)
    {
        Token = token;
	}
}
