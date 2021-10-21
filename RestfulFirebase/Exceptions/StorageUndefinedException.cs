using System;

namespace RestfulFirebase.Exceptions
{
    /// <summary>
    /// Occurs when there`s an unidentified exception.
    /// </summary>
    public class StorageUndefinedException : StorageException
    {
        internal StorageUndefinedException()
            : this(null)
        {

        }

        internal StorageUndefinedException(Exception innerException)
            : base("An unidentified error occured.", innerException)
        {

        }
    }
}
