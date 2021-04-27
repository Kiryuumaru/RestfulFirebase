using System;
using System.Net;
using System.Threading.Tasks;

namespace RestfulFirebase.Database
{
    public class FirebaseDatabaseException : Exception
    {
        public string RequestData { get; }

        public string RequestUrl { get; }

        public string ResponseData { get; }

        public HttpStatusCode StatusCode { get; }

        public bool TaskCancelled { get; }

        public FirebaseDatabaseException(string requestUrl, string requestData, string responseData, HttpStatusCode statusCode)
            : base(GenerateExceptionMessage(requestUrl, requestData, responseData))
        {
            RequestUrl = requestUrl;
            RequestData = requestData;
            ResponseData = responseData;
            StatusCode = statusCode;
        }

        public FirebaseDatabaseException(string requestUrl, string requestData, string responseData, HttpStatusCode statusCode, Exception innerException)
            : base(GenerateExceptionMessage(requestUrl, requestData, responseData), innerException)
        {
            RequestUrl = requestUrl;
            RequestData = requestData;
            ResponseData = responseData;
            StatusCode = statusCode;
            if (innerException is TaskCanceledException)
            {
                TaskCancelled = true;
            }
        }

        private static string GenerateExceptionMessage(string requestUrl, string requestData, string responseData)
        {
            return $"Exception occured while processing the request.\nUrl: {requestUrl}\nRequest Data: {requestData}\nResponse: {responseData}";
        }
    }
}
