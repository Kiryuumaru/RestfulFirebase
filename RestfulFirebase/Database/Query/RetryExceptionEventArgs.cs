using System;
using System.Collections.Generic;
using System.Text;

namespace RestfulFirebase.Database.Query
{
    public class RetryExceptionEventArgs<T>
        where T : Exception
    {
        public readonly T Exception;
        public bool Retry { get; set; }

        public RetryExceptionEventArgs(T exception, bool retry = false)
        {
            Exception = exception;
            Retry = retry;
        }
    }
}
