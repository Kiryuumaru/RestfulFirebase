using System;

namespace RestfulFirebase.Exceptions;

/// <summary>
/// Occurs when the specified Realtime Database was not found.
/// </summary>
public class DatabaseNotFoundException : DatabaseException
{
    private const string ExceptionMessage =
        "The specified Realtime Database was not found.";

    /// <summary>
    /// Creates an instance of <see cref="DatabaseNotFoundException"/>.
    /// </summary>
    public DatabaseNotFoundException()
        : base(ExceptionMessage)
    {

    }

    /// <summary>
    /// Creates an instance of <see cref="DatabaseNotFoundException"/> with provided <paramref name="innerException"/>.
    /// </summary>
    /// <param name="innerException">
    /// The inner exception occured.
    /// </param>
    public DatabaseNotFoundException(Exception innerException)
        : base(ExceptionMessage, innerException)
    {

    }
}
