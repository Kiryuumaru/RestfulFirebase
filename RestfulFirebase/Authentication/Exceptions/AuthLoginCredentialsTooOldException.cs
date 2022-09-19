using System;

namespace RestfulFirebase.Common.Exceptions;

/// <summary>
/// Occurs when the user's credential is no longer valid. The user must sign in again.
/// </summary>
public class AuthLoginCredentialsTooOldException : AuthException
{
    private const string ExceptionMessage =
        "The user's credential is no longer valid. The user must sign in again.";

    /// <summary>
    /// Creates an instance of <see cref="AuthLoginCredentialsTooOldException"/>.
    /// </summary>
    public AuthLoginCredentialsTooOldException()
        : base(ExceptionMessage)
    {

    }

    /// <summary>
    /// Creates an instance of <see cref="AuthLoginCredentialsTooOldException"/> with provided <paramref name="innerException"/>.
    /// </summary>
    /// <param name="innerException">
    /// The inner exception occured.
    /// </param>
    public AuthLoginCredentialsTooOldException(Exception innerException)
        : base(ExceptionMessage, innerException)
    {

    }
}
