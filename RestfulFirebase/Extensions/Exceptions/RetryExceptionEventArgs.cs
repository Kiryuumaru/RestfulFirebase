using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RestfulFirebase.Extensions
{
    public class RetryExceptionEventArgs : EventArgs
    {
        public readonly Exception Exception;
        public Task<bool> Retry { get; set; }

        public RetryExceptionEventArgs(Exception exception, Task<bool> retry = null)
        {
            Exception = exception;
            Retry = retry;
        }
    }
}
