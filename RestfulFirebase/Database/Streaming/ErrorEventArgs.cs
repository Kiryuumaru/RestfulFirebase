using System;

namespace RestfulFirebase.Database.Streaming;

internal class ErrorEventArgs : EventArgs
{
    public string Url { get; }
    public Exception Exception { get; }

    public ErrorEventArgs(string url, Exception exception)
    {
        Url = url;
        Exception = exception;
    }
}
