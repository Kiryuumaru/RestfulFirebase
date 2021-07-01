using System;
using System.Collections.Generic;
using System.Text;

namespace RestfulFirebase.Exceptions
{
    /// <summary>
    /// Occurs when the provided API key is not valid.
    /// </summary>
    public class AuthAPIKeyNotValidException : AuthException
    {
        internal AuthAPIKeyNotValidException()
            : this(null)
        {

        }

        internal AuthAPIKeyNotValidException(Exception innerException)
            : base("API key is not valid. Please pass a valid API key.", innerException)
        {

        }
    }
}
