using System;

namespace RestfulFirebase.Exceptions;

/// <summary>
/// Occurs when the provided model is not valid.
/// </summary>
public class DatabaseInvalidModel : DatabaseException
{
    private const string ExceptionMessage =
        "The provided model is not valid.";

    /// <summary>
    /// Creates an instance of <see cref="DatabaseInvalidModel"/>.
    /// </summary>
    public DatabaseInvalidModel()
        : base(ExceptionMessage)
    {

    }

    /// <summary>
    /// Creates an instance of <see cref="DatabaseInvalidModel"/> with provided <paramref name="innerException"/>.
    /// </summary>
    /// <param name="innerException">
    /// The inner exception occured.
    /// </param>
    public DatabaseInvalidModel(Exception innerException)
        : base(ExceptionMessage, innerException)
    {

    }
}
