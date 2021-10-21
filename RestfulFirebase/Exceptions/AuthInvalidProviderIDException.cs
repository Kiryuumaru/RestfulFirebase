using System;

namespace RestfulFirebase.Exceptions
{
    /// <summary>
    /// Occurs when the supported provider identifier string is not a valid providerId.
    /// </summary>
    public class AuthInvalidProviderIDException : AuthException
    {
        internal AuthInvalidProviderIDException()
            : this(null)
        {

        }

        internal AuthInvalidProviderIDException(Exception innerException)
            : base("The providerId must be a valid supported provider identifier string.", innerException)
        {

        }
    }
}
