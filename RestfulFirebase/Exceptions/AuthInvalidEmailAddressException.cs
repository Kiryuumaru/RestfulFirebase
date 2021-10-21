using System;

namespace RestfulFirebase.Exceptions
{
    /// <summary>
    /// Occurs when the email address is badly formatted.
    /// </summary>
    public class AuthInvalidEmailAddressException : AuthException
    {
        internal AuthInvalidEmailAddressException()
            : this(null)
        {

        }

        internal AuthInvalidEmailAddressException(Exception innerException)
            : base("The email address is badly formatted.", innerException)
        {

        }
    }
}
