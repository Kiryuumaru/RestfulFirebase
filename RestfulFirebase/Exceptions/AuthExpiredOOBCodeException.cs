using System;
using System.Collections.Generic;
using System.Text;

namespace RestfulFirebase.Exceptions
{
    /// <summary>
    /// Occurs when the action code has expired.
    /// </summary>
    public class AuthExpiredOOBCodeException : AuthException
    {
        internal AuthExpiredOOBCodeException()
            : this(null)
        {

        }

        internal AuthExpiredOOBCodeException(Exception innerException)
            : base("The action code has expired.", innerException)
        {

        }
    }
}
