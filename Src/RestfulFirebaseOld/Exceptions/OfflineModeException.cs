using System;

namespace RestfulFirebase.Exceptions;

/// <summary>
/// Occurs when the operation is not executed because the offline mode is enabled.
/// </summary>
public class OfflineModeException : Exception
{
    private const string ExceptionMessage =
        "The operation is not executed because the offline mode is enabled.";

    /// <summary>
    /// Creates an instance of <see cref="OfflineModeException"/>.
    /// </summary>
    public OfflineModeException()
        : base(ExceptionMessage)
    {

    }

    /// <summary>
    /// Creates an instance of <see cref="OfflineModeException"/> with provided <paramref name="innerException"/>.
    /// </summary>
    /// <param name="innerException">
    /// The inner exception occured.
    /// </param>
    public OfflineModeException(Exception innerException)
        : base(ExceptionMessage, innerException)
    {

    }
}
