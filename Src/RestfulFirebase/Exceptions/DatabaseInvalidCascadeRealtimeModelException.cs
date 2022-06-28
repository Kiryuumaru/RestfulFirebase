using System;

namespace RestfulFirebase.Exceptions;

/// <summary>
/// Occurs when the cascade IRealtimeModel has no parameterless constructor but there`s no provided default value.
/// </summary>
public class DatabaseInvalidCascadeRealtimeModelException : DatabaseException
{
    private const string ExceptionMessage =
        "Cascade IRealtimeModel with no parameterless constructor should have a default value.";

    /// <summary>
    /// Creates an instance of <see cref="DatabaseInvalidCascadeRealtimeModelException"/>.
    /// </summary>
    public DatabaseInvalidCascadeRealtimeModelException()
        : base(ExceptionMessage)
    {

    }

    /// <summary>
    /// Creates an instance of <see cref="DatabaseInvalidCascadeRealtimeModelException"/> with provided <paramref name="innerException"/>.
    /// </summary>
    /// <param name="innerException">
    /// The inner exception occured.
    /// </param>
    public DatabaseInvalidCascadeRealtimeModelException(Exception innerException)
        : base(ExceptionMessage, innerException)
    {

    }
}
