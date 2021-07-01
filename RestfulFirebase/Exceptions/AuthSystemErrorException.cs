using System;
using System.Collections.Generic;
using System.Text;

namespace RestfulFirebase.Exceptions
{
    /// <summary>
    /// Occurs when the system has error.
    /// </summary>
    public class AuthSystemErrorException : AuthException
    {
        internal AuthSystemErrorException()
            : this(null)
        {

        }

        internal AuthSystemErrorException(Exception innerException)
            : base("A system error has occurred.", innerException)
        {

        }
    }
}
