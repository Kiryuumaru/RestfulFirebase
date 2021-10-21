using System;

namespace RestfulFirebase.Exceptions
{
    /// <summary>
    /// Occurs when an email address was expected but one was not provided.
    /// </summary>
    public class AuthMissingEmailException : AuthException
    {
        internal AuthMissingEmailException()
            : this(null)
        {

        }

        internal AuthMissingEmailException(Exception innerException)
            : base("Email address was expected but one was not provided.", innerException)
        {

        }
    }
}
