using System;
using System.Collections.Generic;
using System.Text;

namespace RestfulFirebase.Exceptions
{
    /// <summary>
    /// Occurs when the custom token format is incorrect or the token is invalid for some reason (e.g. expired, invalid signature etc.)
    /// </summary>
    public class AuthInvalidCustomTokenException : AuthException
    {
        internal AuthInvalidCustomTokenException()
            : this(null)
        {

        }

        internal AuthInvalidCustomTokenException(Exception innerException)
            : base("The custom token format is incorrect or the token is invalid for some reason (e.g. expired, invalid signature etc.)", innerException)
        {

        }
    }
}
