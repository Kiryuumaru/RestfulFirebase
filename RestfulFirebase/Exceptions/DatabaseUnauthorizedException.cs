using System;

namespace RestfulFirebase.Exceptions
{
    /// <summary>
    /// Occurs when the request is not authorized by database rules.
    /// </summary>
    public class DatabaseUnauthorizedException : DatabaseException
    {
        internal DatabaseUnauthorizedException()
            : this(null)
        {

        }

        internal DatabaseUnauthorizedException(Exception innerException)
            : base("The request is not authorized by database rules.", innerException)
        {

        }
    }
}
