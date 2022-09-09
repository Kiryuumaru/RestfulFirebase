using System;

namespace RestfulFirebase.Exceptions;

/// <summary>
/// Occurs when storage bucket is required but one was not configured in <see cref="RestfulFirebaseApp.Config"/>.
/// </summary>
public class StorageBucketMissingException : StorageException
{
    private const string ExceptionMessage =
        "The storage bucket is required but one was not configured in config.";

    /// <summary>
    /// Creates an instance of <see cref="DatabaseUrlMissingException"/>.
    /// </summary>
    public StorageBucketMissingException()
        : base(ExceptionMessage)
    {

    }

    /// <summary>
    /// Creates an instance of <see cref="DatabaseUrlMissingException"/> with provided <paramref name="innerException"/>.
    /// </summary>
    /// <param name="innerException">
    /// The inner exception occured.
    /// </param>
    public StorageBucketMissingException(Exception innerException)
        : base(ExceptionMessage, innerException)
    {

    }
}
