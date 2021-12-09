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
                case HttpStatusCode.BadRequest:                 //400
                    return new DatabaseBadRequestException(originalException);
                case HttpStatusCode.Unauthorized:               //401
                    return new DatabaseUnauthorizedException(originalException);
                case HttpStatusCode.PaymentRequired:            //402
                    return new DatabasePaymentRequiredException(originalException);
                case HttpStatusCode.NotFound:                   //404
                    return new DatabaseNotFoundException(originalException);
                case HttpStatusCode.PreconditionFailed:         //412
                    return new DatabasePreconditionFailedException(originalException);
                case HttpStatusCode.InternalServerError:        //500
                    return new DatabaseInternalServerErrorException(originalException);
                case HttpStatusCode.ServiceUnavailable:         //503
                    return new DatabaseServiceUnavailableException(originalException);
                default:
                    return new DatabaseUndefinedException(originalException, statusCode);
            }
        }
    }
}
