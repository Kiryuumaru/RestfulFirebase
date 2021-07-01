using System;
using System.Collections.Generic;
using System.Text;

namespace RestfulFirebase.Exceptions
{
    /// <summary>
    /// Occurs when the supplied auth credential is malformed or has expired.
    /// </summary>
    public class AuthInvalidIDPResponseException : AuthException
    {
        internal AuthInvalidIDPResponseException()
            : this(null)
        {

        }

        internal AuthInvalidIDPResponseException(Exception innerException)
            : base("The supplied auth credential is malformed or has expired.", innerException)
        {

        }
    }
}
