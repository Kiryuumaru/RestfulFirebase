using System;

namespace RestfulFirebase.Exceptions;

/// <summary>
/// Occurs when there`s an error in authentication.
/// </summary>
public abstract class AuthException : Exception
{
    private protected AuthException()
    {

    }

    private protected AuthException(Exception innerException)
        : base("An authentication error occured.", innerException)
    {

    }

    private protected AuthException(string message)
        : base(message)
    {

    }

    private protected AuthException(string message, Exception innerException)
        : base(message, innerException)
    {

    }
}
