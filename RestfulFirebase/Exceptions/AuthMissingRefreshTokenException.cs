using System;

namespace RestfulFirebase.Exceptions
{
    /// <summary>
    /// Occurs when token was expected but one was not provided.
    /// </summary>
    public class AuthMissingRefreshTokenException : AuthException
    {
        internal AuthMissingRefreshTokenException()
            : this(null)
        {

        }

        internal AuthMissingRefreshTokenException(Exception innerException)
            : base("Token was expected but one was not provided.", innerException)
        {

        }
    }
}
