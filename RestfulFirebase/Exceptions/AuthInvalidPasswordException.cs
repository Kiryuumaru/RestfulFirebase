using System;

namespace RestfulFirebase.Exceptions
{
    /// <summary>
    /// Occurs when the password is invalid or the user does not have a password.
    /// </summary>
    public class AuthInvalidPasswordException : AuthException
    {
        internal AuthInvalidPasswordException()
            : this(null)
        {

        }

        internal AuthInvalidPasswordException(Exception innerException)
            : base("The password is invalid or the user does not have a password.", innerException)
        {

        }
    }
}
