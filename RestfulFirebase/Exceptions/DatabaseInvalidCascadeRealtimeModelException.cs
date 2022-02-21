using System;

namespace RestfulFirebase.Exceptions;

/// <summary>
/// Occurs when the cascade IRealtimeModel has no parameterless constructor but there`s no provided default value.
/// </summary>
public class DatabaseInvalidCascadeRealtimeModelException : DatabaseException
{
    private const string ExceptionMessage =
        "Cascade IRealtimeModel with no parameterless constructor should have a default value.";

    internal DatabaseInvalidCascadeRealtimeModelException()
        : base(ExceptionMessage)
    {

    }

    internal DatabaseInvalidCascadeRealtimeModelException(Exception innerException)
        : base(ExceptionMessage, innerException)
    {

    }
}
