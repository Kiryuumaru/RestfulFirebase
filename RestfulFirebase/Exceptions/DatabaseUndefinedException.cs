using System;

namespace RestfulFirebase.Exceptions
{
    /// <summary>
    /// Occurs when there`s an unidentified exception.
    /// </summary>
    public class DatabaseUndefinedException : DatabaseException
    {
        internal DatabaseUndefinedException()
            : this(default(Exception))
        {

        }

        internal DatabaseUndefinedException(string message)
            : base(message)
        {

        }

        internal DatabaseUndefinedException(Exception innerException)
            : base("An unidentified error occured.", innerException)
        {

        }

        internal DatabaseUndefinedException(string message, Exception innerException)
            : base(message, innerException)
        {

        }
    }
}
