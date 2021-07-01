using System;
using System.Collections.Generic;
using System.Text;

namespace RestfulFirebase.Exceptions
{
    /// <summary>
    /// Occurs when there is no user record corresponding to the identifier. The user may have been deleted.
    /// </summary>
    public class AuthUserNotFoundException : AuthException
    {
        internal AuthUserNotFoundException()
            : this(null)
        {

        }

        internal AuthUserNotFoundException(Exception innerException)
            : base("There is no user record corresponding to this identifier. The user may have been deleted.", innerException)
        {

        }
    }
}
