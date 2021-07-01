using System;
using System.Collections.Generic;
using System.Text;

namespace RestfulFirebase.Exceptions
{
    /// <summary>
    /// Occurs when an specified operation is disabled for this project.
    /// </summary>
    public class AuthOperationNotAllowedException : AuthException
    {
        internal AuthOperationNotAllowedException()
            : this(null)
        {

        }

        internal AuthOperationNotAllowedException(Exception innerException)
            : base("Specified operation is disabled for this project.", innerException)
        {

        }
    }
}
