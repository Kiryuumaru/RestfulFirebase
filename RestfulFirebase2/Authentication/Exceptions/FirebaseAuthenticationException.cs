using RestfulFirebase.Authentication.Enums;
using RestfulFirebase.Common.Exceptions;
using System;
using System.Net;

namespace RestfulFirebase.Authentication.Exceptions;

/// <summary>
/// The exception for firebase authentication exceptions.
/// </summary>
public class FirebaseAuthenticationException : FirebaseException
{
    /// <summary>
    /// Gets the <see cref="AuthErrorType"/> of the exception.
    /// </summary>
    public AuthErrorType ErrorType { get; }

    internal FirebaseAuthenticationException(AuthErrorType errorType, string message, string? requestUrl, string? requestContent, string? response, HttpStatusCode httpStatusCode, Exception? innerException)
        : base(message, requestUrl, requestContent, response, httpStatusCode, innerException)
    {
        ErrorType = errorType;
    }
}
