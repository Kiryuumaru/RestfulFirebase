using System;

namespace RestfulFirebase.Database.Realtime;

/// <summary>
/// Occurs when there`s a realtime instance error.
/// </summary>
public class WireExceptionEventArgs : EventArgs
{
    /// <summary>
    /// The Uri of the exception
    /// </summary>
    public string Uri { get; }

    /// <summary>
    /// The exception occured.
    /// </summary>
    public Exception Exception { get; }

    internal WireExceptionEventArgs(string uri, Exception exception)
    {
        Uri = uri;
        Exception = exception;
    }
}
