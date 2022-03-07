using System;

namespace RestfulFirebase.Exceptions;

/// <summary>
/// Occurs when there`s an internal server error.
/// </summary>
public class DatabaseInternalServerErrorException : DatabaseException
{
    private const string ExceptionMessage =
        "An internal server error occured.";

    /// <summary>
    /// Creates an instance of <see cref="DatabaseInternalServerErrorException"/>.
    /// </summary>
    public DatabaseInternalServerErrorException()
        : base(ExceptionMessage)
    {

    }

    /// <summary>
    /// Creates an instance of <see cref="DatabaseInternalServerErrorException"/> with provided <paramref name="innerException"/>.
    /// </summary>
    /// <param name="innerException">
    /// The inner exception occured.
    /// </param>
    public DatabaseInternalServerErrorException(Exception innerException)
        : base(ExceptionMessage, innerException)
    {

    }
}
