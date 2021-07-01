using System;
using System.Collections.Generic;
using System.Text;

namespace RestfulFirebase.Exceptions
{
    /// <summary>
    /// Occurs when the supplied auth credential is malformed or has expired.
    /// </summary>
    public class AuthStaleIDTokenException : AuthException
    {
        internal AuthStaleIDTokenException()
            : this(null)
        {

        }

        internal AuthStaleIDTokenException(Exception innerException)
            : base("The supplied auth credential is malformed or has expired.", innerException)
        {

        }
    }
}
