using RestfulFirebase.Exceptions;
using RestfulFirebase.Utilities;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace RestfulFirebase.Database
{
    internal class ExceptionHelpers
    {
        internal static Exception GetException(HttpStatusCode statusCode, Exception originalException)
        {
            switch (statusCode)
            {
                case HttpStatusCode.BadRequest:
                    return new DatabaseBadRequestException(originalException);
                case HttpStatusCode.Unauthorized:
                    return new DatabaseUnauthorizedException(originalException);
                case HttpStatusCode.NotFound:
                    return new DatabaseNotFoundException(originalException);
                case HttpStatusCode.InternalServerError:
                    return new DatabaseInternalServerErrorException(originalException);
                case HttpStatusCode.ServiceUnavailable:
                    return new DatabaseServiceUnavailableException(originalException);
                case HttpStatusCode.PreconditionFailed:
                    return new DatabasePreconditionFailedException(originalException);
                default:
                    return new DatabaseUndefinedException(originalException);
            }
        }
    }
}
