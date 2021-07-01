using System;
using System.Collections.Generic;
using System.Text;

namespace RestfulFirebase.Exceptions
{
    /// <summary>
    /// Occurs when the specified Realtime Database was not found.
    /// </summary>
    public class DatabaseNotFoundException : DatabaseException
    {
        internal DatabaseNotFoundException()
            : this(null)
        {

        }

        internal DatabaseNotFoundException(Exception innerException)
            : base("The specified Realtime Database was not found.", innerException)
        {

        }
    }
}
