using System;

namespace RestfulFirebase.Exceptions
{
    /// <summary>
    /// Occurs when request uri was expected but one was not provided.
    /// </summary>
    public class AuthMissingRequestURIException : AuthException
    {
        internal AuthMissingRequestURIException()
            : this(null)
        {

        }

        internal AuthMissingRequestURIException(Exception innerException)
            : base("Request uri was expected but one was not provided.", innerException)
        {

        }
    }
}
