using System;

namespace RestfulFirebase.Exceptions
{
    /// <summary>
    /// Occurs when the user's credential is no longer valid. The user must sign in again.
    /// </summary>
    public class AuthTokenExpiredException : AuthException
    {
        internal AuthTokenExpiredException()
            : this(null)
        {

        }

        internal AuthTokenExpiredException(Exception innerException)
            : base("The user's credential is no longer valid. The user must sign in again.", innerException)
        {

        }
    }
}
