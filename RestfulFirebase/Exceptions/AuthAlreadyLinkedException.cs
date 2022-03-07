using System;

namespace RestfulFirebase.Exceptions;

/// <summary>
/// Occurs when the specified credential is already associated with a different user account.
/// </summary>
public class AuthAlreadyLinkedException : AuthException
{
    private const string ExceptionMessage =
        "This credential is already associated with a different user account.";

    /// <summary>
    /// Creates an instance of <see cref="AuthAlreadyLinkedException"/>.
    /// </summary>
    public AuthAlreadyLinkedException()
        : base(ExceptionMessage)
    {

    }

    /// <summary>
    /// Creates an sinstance of <see cref="AuthAlreadyLinkedException"/> with provided <paramref name="innerException"/>.
    /// </summary>
    /// <param name="innerException">
    /// The inner exception occured.
    /// </param>
    public AuthAlreadyLinkedException(Exception innerException)
        : base(ExceptionMessage, innerException)
    {

    }
}
