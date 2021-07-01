using System;
using System.Collections.Generic;
using System.Text;

namespace RestfulFirebase.Exceptions
{
    /// <summary>
    /// Occurs when either the user or API keys are incorrect, or the API key has expired.
    /// </summary>
    public class AuthInvalidAccessTokenException : AuthException
    {
        internal AuthInvalidAccessTokenException()
            : this(null)
        {

        }

        internal AuthInvalidAccessTokenException(Exception innerException)
            : base("Either the user or API keys are incorrect, or the API key has expired.", innerException)
        {

        }
    }
}
