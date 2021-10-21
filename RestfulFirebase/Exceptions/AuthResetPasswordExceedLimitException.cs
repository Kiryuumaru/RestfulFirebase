using System;

namespace RestfulFirebase.Exceptions
{
    /// <summary>
    /// Occurs when the reset password request exceeds its limit.
    /// </summary>
    public class AuthResetPasswordExceedLimitException : AuthException
    {
        internal AuthResetPasswordExceedLimitException()
            : this(null)
        {

        }

        internal AuthResetPasswordExceedLimitException(Exception innerException)
            : base("The reset password request exceeds its limit.", innerException)
        {

        }
    }
}
