using System;

namespace RestfulFirebase.Exceptions
{
    /// <summary>
    /// Occurs when there is an unusual activity on device.
    /// </summary>
    public class AuthTooManyAttemptsException : AuthException
    {
        internal AuthTooManyAttemptsException()
            : this(null)
        {

        }

        internal AuthTooManyAttemptsException(Exception innerException)
            : base("We have blocked all requests from this device due to unusual activity. Try again later.", innerException)
        {

        }
    }
}
