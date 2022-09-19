using System;

namespace RestfulFirebase.Common.Exceptions;

/// <summary>
/// Occurs when request uri was expected but one was not provided.
/// </summary>
public class AuthMissingRequestURIException : AuthException
{
    private const string ExceptionMessage =
        "Request uri was expected but one was not provided.";

    /// <summary>
    /// Creates an instance of <see cref="AuthMissingRequestURIException"/>.
    /// </summary>
    public AuthMissingRequestURIException()
        : base(ExceptionMessage)
    {

    }

    /// <summary>
    /// Creates an instance of <see cref="AuthMissingRequestURIException"/> with provided <paramref name="innerException"/>.
    /// </summary>
    /// <param name="innerException">
    /// The inner exception occured.
    /// </param>
    public AuthMissingRequestURIException(Exception innerException)
        : base(ExceptionMessage, innerException)
    {

    }
}
