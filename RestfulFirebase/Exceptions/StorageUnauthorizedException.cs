using System;

namespace RestfulFirebase.Exceptions
{
    /// <summary>
    /// Occurs when the request is not authorized by storage rules.
    /// </summary>
    public class StorageUnauthorizedException : StorageException
    {
        internal StorageUnauthorizedException()
            : this(null)
        {

        }

        internal StorageUnauthorizedException(Exception innerException)
            : base("The request is not authorized by storage rules.", innerException)
        {

        }
    }
}
