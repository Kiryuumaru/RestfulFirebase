using System;

namespace RestfulFirebase.Exceptions
{
    /// <summary>
    /// Occurs when there is no user record corresponding to the identifier. The user may have been deleted.
    /// </summary>
    public class AuthEmailNotFoundException : AuthException
    {
        internal AuthEmailNotFoundException()
            : this(null)
        {

        }

        internal AuthEmailNotFoundException(Exception innerException)
            : base("There is no user record corresponding to this identifier. The user may have been deleted.", innerException)
        {

        }
    }
}
