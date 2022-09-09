using System;

namespace RestfulFirebase.Exceptions;

/// <summary>
/// Occurs when the request is not authorized by storage rules.
/// </summary>
public class StorageUnauthorizedException : StorageException
{
    private const string ExceptionMessage =
        "The request is not authorized by storage rules.";

    /// <summary>
    /// Creates an instance of <see cref="StorageUnauthorizedException"/>.
    /// </summary>
    public StorageUnauthorizedException()
        : base(ExceptionMessage)
    {

    }

    /// <summary>
    /// Creates an instance of <see cref="StorageUnauthorizedException"/> with provided <paramref name="innerException"/>.
    /// </summary>
    /// <param name="innerException">
    /// The inner exception occured.
    /// </param>
    public StorageUnauthorizedException(Exception innerException)
        : base(ExceptionMessage, innerException)
    {

    }
}
