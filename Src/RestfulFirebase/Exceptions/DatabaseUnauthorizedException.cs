using System;

namespace RestfulFirebase.Exceptions;

/// <summary>
/// Occurs when the request is not authorized by database rules.
/// </summary>
public class DatabaseUnauthorizedException : DatabaseException
{
    private const string ExceptionMessage =
        "The request is not authorized by database rules.";

    /// <summary>
    /// Creates an instance of <see cref="DatabaseUnauthorizedException"/>.
    /// </summary>
    public DatabaseUnauthorizedException()
        : base(ExceptionMessage)
    {

    }

    /// <summary>
    /// Creates an instance of <see cref="DatabaseUnauthorizedException"/> with provided <paramref name="innerException"/>.
    /// </summary>
    /// <param name="innerException">
    /// The inner exception occured.
    /// </param>
    public DatabaseUnauthorizedException(Exception innerException)
        : base(ExceptionMessage, innerException)
    {

    }
}
