using RestfulFirebase.Exceptions;
using System;
using System.Net;

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
