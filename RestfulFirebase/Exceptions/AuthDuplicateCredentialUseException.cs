using System;

namespace RestfulFirebase.Exceptions
{
    /// <summary>
    /// Occurs when the API request was received with a repeated or lower than expected nonce value.
    /// </summary>
    public class AuthDuplicateCredentialUseException : AuthException
    {
        internal AuthDuplicateCredentialUseException()
            : this(null)
        {

        }

        internal AuthDuplicateCredentialUseException(Exception innerException)
            : base("API request was received with a repeated or lower than expected nonce value.", innerException)
        {

        }
    }
}
