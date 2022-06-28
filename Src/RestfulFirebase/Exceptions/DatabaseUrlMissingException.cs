using System;

namespace RestfulFirebase.Exceptions;

/// <summary>
/// Occurs when database URL is required but one was not configured in <see cref="RestfulFirebaseApp.Config"/>.
/// </summary>
public class DatabaseUrlMissingException : DatabaseException
{
    private const string ExceptionMessage =
        "The database URL is required but one was not configured in config.";

    /// <summary>
    /// Creates an instance of <see cref="DatabaseUrlMissingException"/>.
    /// </summary>
    public DatabaseUrlMissingException()
        : base(ExceptionMessage)
    {

    }

    /// <summary>
    /// Creates an instance of <see cref="DatabaseUrlMissingException"/> with provided <paramref name="innerException"/>.
    /// </summary>
    /// <param name="innerException">
    /// The inner exception occured.
    /// </param>
    public DatabaseUrlMissingException(Exception innerException)
        : base(ExceptionMessage, innerException)
    {

    }
}
