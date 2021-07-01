using System;
using System.Collections.Generic;
using System.Text;

namespace RestfulFirebase.Exceptions
{
    /// <summary>
    /// Occurs when the request's specified ETag value in the if-match header did not match the server's value.
    /// </summary>
    public class DatabasePreconditionFailedException : DatabaseException
    {
        internal DatabasePreconditionFailedException()
            : this(null)
        {

        }

        internal DatabasePreconditionFailedException(Exception innerException)
            : base("The request's specified ETag value in the if-match header did not match the server's value.", innerException)
        {

        }
    }
}
