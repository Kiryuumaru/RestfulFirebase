using System;

namespace RestfulFirebase.Exceptions
{
    /// <summary>
    /// Occurs when the user's credential is no longer valid. The user must sign in again.
    /// </summary>
    public class AuthLoginCredentialsTooOldException : AuthException
    {
        internal AuthLoginCredentialsTooOldException()
            : this(null)
        {

        }

        internal AuthLoginCredentialsTooOldException(Exception innerException)
            : base("The user's credential is no longer valid. The user must sign in again.", innerException)
        {

        }
    }
}
