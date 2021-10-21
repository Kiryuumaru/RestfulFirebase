using System;

namespace RestfulFirebase.Exceptions
{
    /// <summary>
    /// Occurs when an identifier was expected but one was not provided.
    /// </summary>
    public class AuthMissingIdentifierException : AuthException
    {
        internal AuthMissingIdentifierException()
            : this(null)
        {

        }

        internal AuthMissingIdentifierException(Exception innerException)
            : base("Identifier was expected but one was not provided.", innerException)
        {

        }
    }
}
