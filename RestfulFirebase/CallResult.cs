using System;
using System.Collections.Generic;
using System.Text;

namespace RestfulFirebase
{
    public class CallResult
    {
        public bool IsSuccess { get; protected set; }
        public Exception Exception { get; protected set; }

        public static CallResult Success()
        {
            return new CallResult(true);
        }

        public static CallResult<TResult> Success<TResult>(TResult result)
        {
            return new CallResult<TResult>(result, true);
        }

        public static CallResult Error()
        {
            return new CallResult(false);
        }

        public static CallResult Error(Exception exception)
        {
            return new CallResult(false, exception);
        }

        public static CallResult<TResult> Error<TResult>(Exception exception)
        {
            return new CallResult<TResult>(default, false, exception);
        }

        public static CallResult<TResult> Error<TResult>(TResult result, Exception exception)
        {
            return new CallResult<TResult>(result, false, exception);
        }

        public CallResult(bool isSuccess, Exception exception = null)
        {
            IsSuccess = isSuccess;
            Exception = exception;
        }
    }

    public class CallResult<TResult> : CallResult
    {
        public TResult Result { get; protected set; }

        public CallResult(TResult result, bool isSuccess, Exception exception = null)
            : base(isSuccess, exception)
        {
            Result = result;
        }
    }
}
