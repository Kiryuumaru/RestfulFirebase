using System;
using System.Collections.Generic;
using System.Text;

namespace RestfulFirebase.Exceptions
{
    /// <summary>
    /// Occurs when there`s an internal server error.
    /// </summary>
    public class DatabaseInternalServerErrorException : DatabaseException
    {
        internal DatabaseInternalServerErrorException()
            : this(null)
        {

        }

        internal DatabaseInternalServerErrorException(Exception innerException)
            : base("An internal server error occured.", innerException)
        {

        }
    }
}
