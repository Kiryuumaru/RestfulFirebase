using System;

namespace RestfulFirebase.Exceptions;

/// <summary>
/// Occurs when the cascade IRealtimeModel is null.
/// </summary>
public class DatabaseNullCascadeRealtimeModelException : DatabaseException
{
    private const string ExceptionMessage =
        "Cascade IRealtimeModel cannot be null. Use IRealtimeModel.SetNull() instead.";

    internal DatabaseNullCascadeRealtimeModelException()
        : base(ExceptionMessage)
    {

    }

    internal DatabaseNullCascadeRealtimeModelException(Exception innerException)
        : base(ExceptionMessage, innerException)
    {

    }
}
