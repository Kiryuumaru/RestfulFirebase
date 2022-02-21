using System;

namespace RestfulFirebase.Exceptions;

/// <summary>
/// Occurs when the operation is not executed because the offline mode is enabled.
/// </summary>
public class OfflineModeException : Exception
{
    private const string ExceptionMessage =
        "The operation is not executed because the offline mode is enabled.";

    internal OfflineModeException()
        : base(ExceptionMessage)
    {

    }

    internal OfflineModeException(Exception innerException)
        : base(ExceptionMessage, innerException)
    {

    }
}
