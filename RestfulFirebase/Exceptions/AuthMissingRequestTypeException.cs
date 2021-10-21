using System;

namespace RestfulFirebase.Exceptions
{
    /// <summary>
    /// Occurs when request type was expected but one was not provided.
    /// </summary>
    public class AuthMissingRequestTypeException : AuthException
    {
        internal AuthMissingRequestTypeException()
            : this(null)
        {

        }

        internal AuthMissingRequestTypeException(Exception innerException)
            : base("Request type was expected but one was not provided.", innerException)
        {

        }
    }
}
