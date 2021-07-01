using System;
using System.Collections.Generic;
using System.Text;

namespace RestfulFirebase.Exceptions
{
    /// <summary>
    /// Occurs when the grant type specified is invalid.
    /// </summary>
    public class AuthInvalidGrantTypeException : AuthException
    {
        internal AuthInvalidGrantTypeException()
            : this(null)
        {

        }

        internal AuthInvalidGrantTypeException(Exception innerException)
            : base("The grant type specified is invalid.", innerException)
        {

        }
    }
}
