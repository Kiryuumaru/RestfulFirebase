using System;
using System.Collections.Generic;
using System.Text;

namespace RestfulFirebase.Exceptions
{
    /// <summary>
    /// Occurs when the email address is already in use by another account.
    /// </summary>
    public class AuthEmailExistsException : AuthException
    {
        internal AuthEmailExistsException()
            : this(null)
        {

        }

        internal AuthEmailExistsException(Exception innerException)
            : base("The email address is already in use by another account.", innerException)
        {

        }
    }
}
