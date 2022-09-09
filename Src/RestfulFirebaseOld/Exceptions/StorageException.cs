using System;

namespace RestfulFirebase.Exceptions;

/// <summary>
/// Occurs when there`s an error in storage.
/// </summary>
public class StorageException : Exception
{
    private const string ExceptionMessage =
        "An storage error occured.";

    /// <summary>
    /// Creates an instance of <see cref="StorageException"/>.
    /// </summary>
    public StorageException()
        : base(ExceptionMessage)
    {

    }

    /// <summary>
    /// Creates an instance of <see cref="StorageException"/> with provided <paramref name="innerException"/>.
    /// </summary>
    /// <param name="innerException">
    /// The inner exception occured.
    /// </param>
    public StorageException(Exception innerException)
        : base(ExceptionMessage, innerException)
    {

    }

    /// <summary>
    /// Creates an instance of <see cref="StorageException"/> with provided <paramref name="message"/>.
    /// </summary>
    /// <param name="message">
    /// The message of the exception.
    /// </param>
    public StorageException(string message)
        : base(message)
    {

    }

    /// <summary>
    /// Creates an instance of <see cref="StorageException"/> with provided <paramref name="message"/> and <paramref name="innerException"/>.
    /// </summary>
    /// <param name="message">
    /// The message of the exception.
    /// </param>
    /// <param name="innerException">
    /// The inner exception occured.
    /// </param>
    public StorageException(string message, Exception innerException)
        : base(message, innerException)
    {

    }
}
