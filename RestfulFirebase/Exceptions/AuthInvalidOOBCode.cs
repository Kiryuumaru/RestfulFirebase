using System;

namespace RestfulFirebase.Exceptions
{
    /// <summary>
    /// Occurs when the action code is invalid. This can happen if the code is malformed, expired, or has already been used.
    /// </summary>
    public class AuthInvalidOOBCodeException : AuthException
    {
        internal AuthInvalidOOBCodeException()
            : this(null)
        {

        }

        internal AuthInvalidOOBCodeException(Exception innerException)
            : base("The action code is invalid. This can happen if the code is malformed, expired, or has already been used.", innerException)
        {

        }
    }
}
