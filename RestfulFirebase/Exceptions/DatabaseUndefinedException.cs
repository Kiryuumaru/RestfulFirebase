using System;

namespace RestfulFirebase.Exceptions
{
    /// <summary>
    /// Occurs when there`s an unidentified exception.
    /// </summary>
    public class DatabaseUndefinedException : DatabaseException
    {
        internal DatabaseUndefinedException()
            : this(null)
        {

        }

        internal DatabaseUndefinedException(Exception innerException)
            : base("An unidentified error occured.", innerException)
        {

        }
    }
}
