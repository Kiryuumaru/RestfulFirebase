using System;
using System.Collections.Generic;
using System.Text;

namespace RestfulFirebase.Exceptions
{
    /// <summary>
    /// Occurs when the specified credential is already associated with a different user account.
    /// </summary>
    public class AuthAlreadyLinkedException : AuthException
    {
        internal AuthAlreadyLinkedException()
            : this(null)
        {

        }

        internal AuthAlreadyLinkedException(Exception innerException)
            : base("This credential is already associated with a different user account.", innerException)
        {

        }
    }
}
