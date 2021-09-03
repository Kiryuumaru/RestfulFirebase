using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RestfulFirebase.Utilities
{
    /// <summary>
    /// Event arguments for retryable executions.
    /// </summary>
    public class RetryExceptionEventArgs : EventArgs
    {
        /// <summary>
        /// The exception throwed at the execution.
        /// </summary>
        public readonly Exception Exception;

        /// <summary>
        /// Gets or sets <c>true</c> whether the execution will retry; otherwise <c>false</c>.
        /// </summary>
        public Task<bool> Retry { get; set; }

        /// <summary>
        /// Creates new instance for <see cref="RetryExceptionEventArgs"/>.
        /// </summary>
        /// <param name="exception">
        /// The exception throwed at the execution.
        /// </param>
        /// <param name="retry">
        /// Specify whether the execution will retry.
        /// </param>
        public RetryExceptionEventArgs(Exception exception, Task<bool> retry = null)
        {
            Exception = exception;
            Retry = retry;
        }
    }
}
