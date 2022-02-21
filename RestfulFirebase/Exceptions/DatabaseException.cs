using System;

namespace RestfulFirebase.Exceptions;

/// <summary>
/// Occurs when there`s an error in realtime database.
/// </summary>
public abstract class DatabaseException : Exception
{
    private protected DatabaseException()
    {

    }

    private protected DatabaseException(Exception innerException)
        : base("A realtime database error occured.", innerException)
    {

    }

    private protected DatabaseException(string message)
        : base(message)
    {

    }

    private protected DatabaseException(string message, Exception innerException)
        : base(message, innerException)
    {

    }
}
