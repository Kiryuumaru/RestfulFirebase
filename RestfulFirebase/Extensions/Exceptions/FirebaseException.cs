using System;
using System.Net;
using System.Threading.Tasks;

namespace RestfulFirebase.Extensions
{
    /// <summary>
    /// The wrapped firebase exception.
    /// </summary>
    public class FirebaseException : Exception
    {
        /// <summary>
        /// The reason of the exception.
        /// </summary>
        public FirebaseExceptionReason Reason { get; }

        internal FirebaseException(FirebaseExceptionReason reason)
            : base()
        {
            Reason = reason;
        }

        internal FirebaseException(FirebaseExceptionReason reason, Exception innerException)
            : base(innerException.Message, innerException)
        {
            Reason = reason;
        }
    }
}
