using System;
using System.Collections.Generic;
using System.Text;

namespace RestfulFirebase.Exceptions
{
    /// <summary>
    /// Occurs when there`s an error in realtime database.
    /// </summary>
    public abstract class DatabaseException : Exception
    {
        private protected DatabaseException()
            : this(null)
        {

        }

        private protected DatabaseException(Exception innerException)
            : base("A realtime database error occured.", innerException)
        {

        }

        private protected DatabaseException(string message, Exception innerException)
            : base(message, innerException)
        {

        }
    }
}
