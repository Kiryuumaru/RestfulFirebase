using System;

namespace RestfulFirebase.Authentication.Exceptions;

/// <summary>
/// Occurs when request type was expected but one was not provided.
/// </summary>
public class AuthMissingRequestTypeException : AuthException
{
    private const string ExceptionMessage =
        "Request type was expected but one was not provided.";

    /// <summary>
    /// Creates an instance of <see cref="AuthMissingRequestTypeException"/>.
    /// </summary>
    public AuthMissingRequestTypeException()
        : base(ExceptionMessage)
    {

    }

    /// <summary>
    /// Creates an instance of <see cref="AuthMissingRequestTypeException"/> with provided <paramref name="innerException"/>.
    /// </summary>
    /// <param name="innerException">
    /// The inner exception occured.
    /// </param>
    public AuthMissingRequestTypeException(Exception innerException)
        : base(ExceptionMessage, innerException)
    {

    }
}
