using System;

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
