using System;
using System.Collections.Generic;
using System.Text;

namespace RestfulFirebase.Exceptions
{
    /// <summary>
    /// Occurs when the specified Firebase Realtime Database is temporarily unavailable, which means the request was not attempted.
    /// </summary>
    public class DatabaseServiceUnavailableException : DatabaseException
    {
        internal DatabaseServiceUnavailableException()
            : this(null)
        {

        }

        internal DatabaseServiceUnavailableException(Exception innerException)
            : base("The specified Firebase Realtime Database is temporarily unavailable, which means the request was not attempted.", innerException)
        {

        }
    }
}
