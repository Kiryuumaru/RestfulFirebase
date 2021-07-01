using System;
using System.Collections.Generic;
using System.Text;

namespace RestfulFirebase.Exceptions
{
    /// <summary>
    /// Occurs when the password is less than 6 characters long.
    /// </summary>
    public class AuthWeakPasswordException : AuthException
    {
        internal AuthWeakPasswordException()
            : this(null)
        {

        }

        internal AuthWeakPasswordException(Exception innerException)
            : base("The password must be 6 characters long or more.", innerException)
        {

        }
    }
}
