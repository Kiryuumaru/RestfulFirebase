using System;

namespace RestfulFirebase.Exceptions
{
    /// <summary>
    /// Occurs when there`s an unidentified exception.
    /// </summary>
    public class AuthUndefinedException : AuthException
    {
        internal AuthUndefinedException()
            : this(null)
        {

        }

        internal AuthUndefinedException(Exception innerException)
            : base("An unidentified exception occurs.", innerException)
        {

        }
    }
}
