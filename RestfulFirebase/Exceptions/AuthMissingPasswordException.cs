using System;

namespace RestfulFirebase.Exceptions
{
    /// <summary>
    /// Occurs when password was expected but one was not provided.
    /// </summary>
    public class AuthMissingPasswordException : AuthException
    {
        internal AuthMissingPasswordException()
            : this(null)
        {

        }

        internal AuthMissingPasswordException(Exception innerException)
            : base("Password was expected but one was not provided.", innerException)
        {

        }
    }
}
