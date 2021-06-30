using System;
using System.Collections.Generic;
using System.Text;

namespace RestfulFirebase
{
    /// <summary>
    /// Provides task results for <see cref="RestfulFirebaseApp"/> with wrapped results and exceptions.
    /// </summary>
    public class CallResult
    {
        /// <summary>
        /// Gets <c>true</c> if the task is successful; otherwise, <c>false</c>.
        /// </summary>
        public bool IsSuccess { get; protected set; }

        /// <summary>
        /// Gets the <see cref="System.Exception"/> throws by the task, if theres any.
        /// </summary>
        public Exception Exception { get; protected set; }

        /// <summary>
        /// Creates new success instance for <see cref="CallResult"/>.
        /// </summary>
        /// <returns>
        /// The created success instance of <see cref="CallResult"/>.
        /// </returns>
        public static CallResult Success()
        {
            return new CallResult(true);
        }

        /// <summary>
        /// Creates new success instance for <see cref="CallResult{TResult}"/>.
        /// </summary>
        /// <typeparam name="TResult">
        /// The underlying type of the result.
        /// </typeparam>
        /// <param name="result">
        /// The value of the result.
        /// </param>
        /// <returns>
        /// The created success instance of <see cref="CallResult{TResult}"/>.
        /// </returns>
        public static CallResult<TResult> Success<TResult>(TResult result)
        {
            return new CallResult<TResult>(result, true);
        }

        /// <summary>
        /// Creates new error instance for <see cref="CallResult"/>.
        /// </summary>
        /// <returns>
        /// The created error instance of <see cref="CallResult"/>.
        /// </returns>
        public static CallResult Error()
        {
            return new CallResult(false);
        }

        /// <summary>
        /// Creates new error instance for <see cref="CallResult"/>.
        /// </summary>
        /// <param name="exception">
        /// The exception of the error <see cref="CallResult"/> instance.
        /// </param>
        /// <returns>
        /// The created error instance of <see cref="CallResult"/>.
        /// </returns>
        public static CallResult Error(Exception exception)
        {
            return new CallResult(false, exception);
        }

        /// <summary>
        /// Creates new error instance for <see cref="CallResult{TResult}"/>.
        /// </summary>
        /// <typeparam name="TResult">
        /// The underlying type of the result.
        /// </typeparam>
        /// <param name="exception">
        /// The exception of the error <see cref="CallResult{TResult}"/> instance.
        /// </param>
        /// <returns>
        /// The created error instance of <see cref="CallResult{TResult}"/>.
        /// </returns>
        public static CallResult<TResult> Error<TResult>(Exception exception)
        {
            return new CallResult<TResult>(default, false, exception);
        }

        /// <summary>
        /// Creates new error instance for <see cref="CallResult{TResult}"/>.
        /// </summary>
        /// <typeparam name="TResult">
        /// The underlying type of the result.
        /// </typeparam>
        /// <param name="result">
        /// The value of the result.
        /// </param>
        /// <param name="exception">
        /// The exception of the error <see cref="CallResult{TResult}"/> instance.
        /// </param>
        /// <returns>
        /// The created error instance of <see cref="CallResult{TResult}"/>.
        /// </returns>
        public static CallResult<TResult> Error<TResult>(TResult result, Exception exception)
        {
            return new CallResult<TResult>(result, false, exception);
        }

        /// <summary>
        /// Creates new instance of <see cref="CallResult"/>.
        /// </summary>
        /// <param name="isSuccess">
        /// The resulting state of the task.
        /// </param>
        /// <param name="exception">
        /// The resulting exception of the task.
        /// </param>
        public CallResult(bool isSuccess, Exception exception = null)
        {
            IsSuccess = isSuccess;
            Exception = exception;
        }
    }

    /// <summary>
    /// Provides task results for <see cref="RestfulFirebaseApp"/> with wrapped results and exceptions.
    /// </summary>
    /// <typeparam name="TResult">
    /// The underlying type of the result.
    /// </typeparam>
    public class CallResult<TResult> : CallResult
    {
        /// <summary>
        /// The result of the task.
        /// </summary>
        public TResult Result { get; protected set; }

        /// <summary>
        /// Creates new instance of <see cref="CallResult{TResult}"/>.
        /// </summary>
        /// <param name="result">
        /// The value of the result.
        /// </param>
        /// <param name="isSuccess">
        /// The resulting state of the task.
        /// </param>
        /// <param name="exception">
        /// The resulting exception of the task.
        /// </param>
        public CallResult(TResult result, bool isSuccess, Exception exception = null)
            : base(isSuccess, exception)
        {
            Result = result;
        }
    }
}
