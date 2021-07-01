using System;
using System.Collections.Generic;
using System.Text;

namespace RestfulFirebase.Exceptions
{
    /// <summary>
    /// Occurs when the custom token corresponds to a different Firebase project.
    /// </summary>
    public class AuthCredentialMismatchException : AuthException
    {
        internal AuthCredentialMismatchException()
            : this(null)
        {

        }

        internal AuthCredentialMismatchException(Exception innerException)
            : base("The custom token corresponds to a different Firebase project.", innerException)
        {

        }
    }
}
