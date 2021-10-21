using System;

namespace RestfulFirebase.Exceptions
{
    /// <summary>
    /// Occurs when the request is malformed.
    /// </summary>
    public class DatabaseBadRequestException : DatabaseException
    {
        internal DatabaseBadRequestException()
            : this(null)
        {

        }

        internal DatabaseBadRequestException(Exception innerException)
            : base("Bad request.", innerException)
        {

        }
    }
}
