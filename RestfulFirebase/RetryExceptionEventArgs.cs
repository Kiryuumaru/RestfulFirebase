using System;
using System.Collections.Generic;
using System.Text;

namespace RestfulFirebase
{
    public class RetryExceptionEventArgs : EventArgs
    {
        public readonly Exception Exception;
        public bool Retry { get; set; }

        public RetryExceptionEventArgs(Exception exception, bool retry = false)
        {
            Exception = exception;
            Retry = retry;
        }
    }
}
