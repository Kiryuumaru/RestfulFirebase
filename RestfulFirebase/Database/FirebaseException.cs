using System;
using System.Net;

namespace RestfulFirebase.Database
{
    public class FirebaseException : Exception
    {
        public string RequestData { get; }

        public string RequestUrl { get; }

        public string ResponseData { get; }

        public HttpStatusCode StatusCode { get; }

        public FirebaseException(string requestUrl, string requestData, string responseData, HttpStatusCode statusCode)
            : base(GenerateExceptionMessage(requestUrl, requestData, responseData))
        {
            RequestUrl = requestUrl;
            RequestData = requestData;
            ResponseData = responseData;
            StatusCode = statusCode;
        }

        public FirebaseException(string requestUrl, string requestData, string responseData, HttpStatusCode statusCode, Exception innerException)
            : base(GenerateExceptionMessage(requestUrl, requestData, responseData), innerException)
        {
            RequestUrl = requestUrl;
            RequestData = requestData;
            ResponseData = responseData;
            StatusCode = statusCode;
        }

        private static string GenerateExceptionMessage(string requestUrl, string requestData, string responseData)
        {
            return $"Exception occured while processing the request.\nUrl: {requestUrl}\nRequest Data: {requestData}\nResponse: {responseData}";
        }
    }
}
