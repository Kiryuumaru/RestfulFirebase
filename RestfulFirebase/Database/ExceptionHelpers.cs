using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace RestfulFirebase.Database
{
    internal class ExceptionHelpers
    {
        internal static FirebaseExceptionReason GetFailureReason(HttpStatusCode statusCode)
        {
            switch (statusCode)
            {
                case HttpStatusCode.BadRequest:
                    return FirebaseExceptionReason.DatabaseBadRequest;
                case HttpStatusCode.Unauthorized:
                    return FirebaseExceptionReason.DatabaseUnauthorized;
                case HttpStatusCode.NotFound:
                    return FirebaseExceptionReason.DatabaseNotFound;
                case HttpStatusCode.InternalServerError:
                    return FirebaseExceptionReason.DatabaseInternalServerError;
                case HttpStatusCode.ServiceUnavailable:
                    return FirebaseExceptionReason.DatabaseServiceUnavailable;
                case HttpStatusCode.PreconditionFailed:
                    return FirebaseExceptionReason.DatabasePreconditionFailed;
            }
            return FirebaseExceptionReason.OfflineMode;
        }
    }
}
