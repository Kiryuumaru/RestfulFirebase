using System;

namespace RestfulFirebase.Exceptions;

/// <summary>
/// Occurs when the request exceeds the database plan limits.
/// </summary>
public class DatabasePaymentRequiredException : DatabaseException
{
    private const string ExceptionMessage =
        "The request exceeds the database plan limits.";

    /// <summary>
    /// Creates an instance of <see cref="DatabasePaymentRequiredException"/>.
    /// </summary>
    public DatabasePaymentRequiredException()
        : base(ExceptionMessage)
    {

    }

    /// <summary>
    /// Creates an instance of <see cref="DatabasePaymentRequiredException"/> with provided <paramref name="innerException"/>.
    /// </summary>
    /// <param name="innerException">
    /// The inner exception occured.
    /// </param>
    public DatabasePaymentRequiredException(Exception innerException)
        : base(ExceptionMessage, innerException)
    {

    }
}
