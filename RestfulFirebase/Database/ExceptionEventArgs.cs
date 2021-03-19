using System;

namespace RestfulFirebase.Database
{
    public class ExceptionEventArgs<T> : EventArgs where T : Exception
    {
        public readonly T Exception;

        public ExceptionEventArgs(T exception)
        {
            Exception = exception;
        }
    }

    public class ExceptionEventArgs : ExceptionEventArgs<Exception>
    {
        public ExceptionEventArgs(Exception exception) : base(exception)
        {
        }
    }

    public class ContinueExceptionEventArgs<T> : ExceptionEventArgs<T> where T: Exception
    {
        public ContinueExceptionEventArgs(T exception, bool ignoreAndContinue) : base(exception)
        {
            IgnoreAndContinue = ignoreAndContinue;
        }

        public bool IgnoreAndContinue
        {
            get;
            set;
        }
    }
}
