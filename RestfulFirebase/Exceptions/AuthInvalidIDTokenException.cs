using System;
using System.Collections.Generic;
using System.Text;

namespace RestfulFirebase.Exceptions
{
    /// <summary>
    /// Occurs when the user's credential is no longer valid. The user must sign in again.
    /// </summary>
    public class AuthInvalidIDTokenException : AuthException
    {
        internal AuthInvalidIDTokenException()
            : this(null)
        {

        }

        internal AuthInvalidIDTokenException(Exception innerException)
            : base("The user's credential is no longer valid. The user must sign in again.", innerException)
        {

        }
    }
}
