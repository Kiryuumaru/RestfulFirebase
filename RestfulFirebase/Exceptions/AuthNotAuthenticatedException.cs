using System;

namespace RestfulFirebase.Exceptions
{
    /// <summary>
    /// Occurs when app is not authenticated.
    /// </summary>
    public class AuthNotAuthenticatedException : AuthException
    {
        internal AuthNotAuthenticatedException()
            : this(null)
        {

        }

        internal AuthNotAuthenticatedException(Exception innerException)
            : base("App is not authenticated.", innerException)
        {

        }
    }
}
