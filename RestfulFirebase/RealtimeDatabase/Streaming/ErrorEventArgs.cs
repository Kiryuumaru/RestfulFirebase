using System;

namespace RestfulFirebase.RealtimeDatabase.Streaming;

internal class StreamError
{
    public string Url { get; }

    public Exception Exception { get; }

    public StreamError(string url, Exception exception)
    {
        Url = url;
        Exception = exception;
    }
}
