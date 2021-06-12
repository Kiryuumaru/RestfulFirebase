using System;
using System.Net;
using System.Threading.Tasks;

namespace RestfulFirebase.Extensions
{
    public class FirebaseException : Exception
    {
        public FirebaseExceptionReason Reason { get; }

        public FirebaseException(FirebaseExceptionReason reason, Exception innerException)
            : base(innerException.Message, innerException)
        {
            Reason = reason;
        }
    }
}
