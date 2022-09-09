using System;

namespace RestfulFirebase.Authentication.Exceptions;

/// <summary>
/// Occurs when there is no user record corresponding to the identifier. The user may have been deleted.
/// </summary>
public class AuthEmailNotFoundException : AuthException
{
    private const string ExceptionMessage =
        "There is no user record corresponding to this identifier. The user may have been deleted.";

    /// <summary>
    /// Creates an instance of <see cref="AuthEmailNotFoundException"/>.
    /// </summary>
    public AuthEmailNotFoundException()
        : base(ExceptionMessage)
    {

    }

    /// <summary>
    /// Creates an instance of <see cref="AuthEmailNotFoundException"/> with provided <paramref name="innerException"/>.
    /// </summary>
    /// <param name="innerException">
    /// The inner exception occured.
    /// </param>
    public AuthEmailNotFoundException(Exception innerException)
        : base(ExceptionMessage, innerException)
    {

    }
}
