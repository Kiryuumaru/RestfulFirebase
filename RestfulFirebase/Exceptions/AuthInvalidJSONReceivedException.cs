using System;
using System.Collections.Generic;
using System.Text;

namespace RestfulFirebase.Exceptions
{
    /// <summary>
    /// Occurs when there`s an invalid JSON payload received.
    /// </summary>
    public class AuthInvalidJSONReceivedException : AuthException
    {
        internal AuthInvalidJSONReceivedException()
            : this(null)
        {

        }

        internal AuthInvalidJSONReceivedException(Exception innerException)
            : base("Invalid JSON payload received.", innerException)
        {

        }
    }
}
