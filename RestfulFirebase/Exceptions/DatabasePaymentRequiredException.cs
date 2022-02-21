using System;

namespace RestfulFirebase.Exceptions;

/// <summary>
/// Occurs when the request exceeds the database plan limits.
/// </summary>
public class DatabasePaymentRequiredException : DatabaseException
{
    private const string ExceptionMessage =
        "The request exceeds the database plan limits.";

    internal DatabasePaymentRequiredException()
        : base(ExceptionMessage)
    {

    }

    internal DatabasePaymentRequiredException(Exception innerException)
        : base(ExceptionMessage, innerException)
    {

    }
}
