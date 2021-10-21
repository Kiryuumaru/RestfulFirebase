using System;

namespace RestfulFirebase.Exceptions
{
    /// <summary>
    /// Occurs when the provided identifier is invalid.
    /// </summary>
    public class AuthInvalidIdentifierException : AuthException
    {
        internal AuthInvalidIdentifierException()
            : this(null)
        {

        }

        internal AuthInvalidIdentifierException(Exception innerException)
            : base("The provided identifier is invalid.", innerException)
        {

        }
    }
}
