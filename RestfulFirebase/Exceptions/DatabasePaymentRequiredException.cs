using System;

namespace RestfulFirebase.Exceptions
{
    /// <summary>
    /// Occurs when the request exceeds the database plan limits.
    /// </summary>
    public class DatabasePaymentRequiredException : DatabaseException
    {
        internal DatabasePaymentRequiredException()
            : this(null)
        {

        }

        internal DatabasePaymentRequiredException(Exception innerException)
            : base("The request exceeds the database plan limits.", innerException)
        {

        }
    }
}
