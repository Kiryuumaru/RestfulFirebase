using System;

namespace RestfulFirebase.Auth
{
    public class FirebaseAuthException : Exception
    {
        public FirebaseAuthException(string requestUrl, string requestData, string responseData, Exception innerException, AuthErrorReason reason = AuthErrorReason.Undefined) 
            : base(GenerateExceptionMessage(requestUrl, requestData, responseData, reason), innerException)
        {
            RequestUrl = requestUrl;
            RequestData = requestData;
            ResponseData = responseData;
            Reason = reason;
        }

        public string RequestData { get; }

        public string RequestUrl { get; }

        public string ResponseData { get; }

        public AuthErrorReason Reason { get; }

        private static string GenerateExceptionMessage(string requestUrl, string requestData, string responseData, AuthErrorReason errorReason)
        {
            return $"Exception occured while authenticating.\nUrl: {requestUrl}\nRequest Data: {requestData}\nResponse: {responseData}\nReason: {errorReason}";
        }
    }
}
