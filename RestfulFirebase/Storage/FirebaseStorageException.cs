using System;

namespace RestfulFirebase.Storage
{
    public class FirebaseStorageException : Exception
    {
        public FirebaseStorageException(string url, string responseData, Exception innerException) : base(GenerateExceptionMessage(url, responseData), innerException)
        {
            RequestUrl = url;
            ResponseData = responseData;
        }

        public string RequestUrl
        {
            get;
            private set;
        }

        public string ResponseData
        {
            get;
            private set;
        }

        private static string GenerateExceptionMessage(string requestUrl, string responseData)
        {
            return $"Exception occured while processing the request.\nUrl: {requestUrl}\nResponse: {responseData}";
        }
    }
}
