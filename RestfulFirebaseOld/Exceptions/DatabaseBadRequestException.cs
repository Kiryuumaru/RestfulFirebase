using System;

namespace RestfulFirebase.Exceptions;

/// <summary>
/// Occurs when the request is malformed.
/// </summary>
public class DatabaseBadRequestException : DatabaseException
{
    private const string ExceptionMessage =
        "Bad request.";

    /// <summary>
    /// Creates an instance of <see cref="DatabaseBadRequestException"/>.
    /// </summary>
    public DatabaseBadRequestException()
        : base(ExceptionMessage)
    {

    }

    /// <summary>
    /// Creates an instance of <see cref="DatabaseBadRequestException"/> with provided <paramref name="innerException"/>.
    /// </summary>
    /// <param name="innerException">
    /// The inner exception occured.
    /// </param>
    public DatabaseBadRequestException(Exception innerException)
        : base(ExceptionMessage, innerException)
    {

    }
}
