using System;

namespace RestfulFirebase.Exceptions;

/// <summary>
/// Occurs when there`s an unidentified exception.
/// </summary>
public class StorageUndefinedException : StorageException
{
    private const string ExceptionMessage =
        "An unidentified error occured.";

    /// <summary>
    /// Creates an instance of <see cref="StorageUndefinedException"/>.
    /// </summary>
    public StorageUndefinedException()
        : base(ExceptionMessage)
    {

    }

    /// <summary>
    /// Creates an instance of <see cref="StorageUndefinedException"/> with provided <paramref name="innerException"/>.
    /// </summary>
    /// <param name="innerException">
    /// The inner exception occured.
    /// </param>
    public StorageUndefinedException(Exception innerException)
        : base(ExceptionMessage, innerException)
    {

    }
}
