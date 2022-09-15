using RestfulFirebase.Exceptions;
using System;
using System.Net;

namespace RestfulFirebase.FirestoreDatabase;

internal class ExceptionHelpers
{
    internal static Exception GetException(HttpStatusCode statusCode, Exception originalException)
    {
        return statusCode switch
        {
            //400
            HttpStatusCode.BadRequest => new DatabaseBadRequestException(originalException),
            //401
            HttpStatusCode.Unauthorized => new DatabaseUnauthorizedException(originalException),
            //402
            HttpStatusCode.PaymentRequired => new DatabasePaymentRequiredException(originalException),
            //403
            HttpStatusCode.Forbidden => new DatabaseUnauthorizedException(originalException),
            //404
            HttpStatusCode.NotFound => new DatabaseNotFoundException(originalException),
            //412
            HttpStatusCode.PreconditionFailed => new DatabasePreconditionFailedException(originalException),
            //500
            HttpStatusCode.InternalServerError => new DatabaseInternalServerErrorException(originalException),
            //503
            HttpStatusCode.ServiceUnavailable => new DatabaseServiceUnavailableException(originalException),
            //Unknown
            _ => new DatabaseUndefinedException(originalException, statusCode),
        };
    }
}
