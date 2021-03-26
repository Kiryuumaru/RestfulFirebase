using System;
using System.Collections.Generic;
using System.Text;

namespace RestfulFirebase.Auth
{
    public class CallResult
    {
        public bool IsSuccess { get; protected set; }

        public static CallResult Success()
        {
            return new CallResult()
            {
                IsSuccess = true
            };
        }

        public static CallResult Error()
        {
            return new CallResult()
            {
                IsSuccess = false
            };
        }
    }

    public class CallResult<TException> : CallResult
        where TException : Exception
    {
        public TException Exception { get; protected set; }

        public CallResult(TException exception)
        {
            Exception = exception;
            IsSuccess = Exception == default;
        }

        public static new CallResult<TException> Success()
        {
            return new CallResult<TException>(null);
        }

        public static CallResult<TException> Error(TException exception)
        {
            return new CallResult<TException>(exception);
        }
    }

    public class CallResult<TResult, TException> : CallResult<TException>
        where TException : Exception
    {
        public TResult Result { get; protected set; }

        public CallResult(TResult result, TException exception) : base(exception)
        {
            Result = result;
        }

        public static CallResult<TResult, TException> Success(TResult result)
        {
            return new CallResult<TResult, TException>(result, null);
        }

        public static new CallResult<TResult, TException> Error(TException exception)
        {
            return new CallResult<TResult, TException>(default, exception);
        }

        public static CallResult<TResult, TException> Error(TResult result, TException exception)
        {
            return new CallResult<TResult, TException>(result, exception);
        }
    }
}
