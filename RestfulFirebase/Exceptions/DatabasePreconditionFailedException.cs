using System;

namespace RestfulFirebase.Exceptions;

/// <summary>
/// Occurs when the request's specified ETag value in the if-match header did not match the server's value.
/// </summary>
public class DatabasePreconditionFailedException : DatabaseException
{
    private const string ExceptionMessage =
        "The request's specified ETag value in the if-match header did not match the server's value.";

    internal DatabasePreconditionFailedException()
        : base(ExceptionMessage)
    {

    }

    internal DatabasePreconditionFailedException(Exception innerException)
        : base(ExceptionMessage, innerException)
    {

    }
}
