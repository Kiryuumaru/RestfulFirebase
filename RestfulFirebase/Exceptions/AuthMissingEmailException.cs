using System;
using System.Collections.Generic;
using System.Text;

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
