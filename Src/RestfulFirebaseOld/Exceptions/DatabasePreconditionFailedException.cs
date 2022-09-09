using System;

namespace RestfulFirebase.Exceptions;

/// <summary>
/// Occurs when the request's specified ETag value in the if-match header did not match the server's value.
/// </summary>
public class DatabasePreconditionFailedException : DatabaseException
{
    private const string ExceptionMessage =
        "The request's specified ETag value in the if-match header did not match the server's value.";

    /// <summary>
    /// Creates an instance of <see cref="DatabasePreconditionFailedException"/>.
    /// </summary>
    public DatabasePreconditionFailedException()
        : base(ExceptionMessage)
    {

    }

    /// <summary>
    /// Creates an instance of <see cref="DatabasePreconditionFailedException"/> with provided <paramref name="innerException"/>.
    /// </summary>
    /// <param name="innerException">
    /// The inner exception occured.
    /// </param>
    public DatabasePreconditionFailedException(Exception innerException)
        : base(ExceptionMessage, innerException)
    {

    }
}
