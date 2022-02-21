using System;

namespace RestfulFirebase.Exceptions;

/// <summary>
/// Occurs when there`s an internal server error.
/// </summary>
public class DatabaseInternalServerErrorException : DatabaseException
{
    private const string ExceptionMessage =
        "An internal server error occured.";

    internal DatabaseInternalServerErrorException()
        : base(ExceptionMessage)
    {

    }

    internal DatabaseInternalServerErrorException(Exception innerException)
        : base(ExceptionMessage, innerException)
    {

    }
}
