using System;

namespace RestfulFirebase.Exceptions
{
    /// <summary>
    /// Occurs when an invalid refresh token is provided.
    /// </summary>
    public class AuthInvalidRefreshTokenException : AuthException
    {
        internal AuthInvalidRefreshTokenException()
            : this(null)
        {

        }

        internal AuthInvalidRefreshTokenException(Exception innerException)
            : base("An invalid refresh token is provided.", innerException)
        {

        }
    }
}
