using System;

namespace RestfulFirebase.Exceptions
{
    /// <summary>
    /// Occurs when the user account has been disabled by an administrator.
    /// </summary>
    public class AuthUserDisabledException : AuthException
    {
        internal AuthUserDisabledException()
            : this(null)
        {

        }

        internal AuthUserDisabledException(Exception innerException)
            : base("The user account has been disabled by an administrator.", innerException)
        {

        }
    }
}
