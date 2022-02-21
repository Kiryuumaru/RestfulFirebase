using System;

namespace RestfulFirebase.Exceptions;

/// <summary>
/// Occurs when the provided model is not valid.
/// </summary>
public class DatabaseInvalidModel : DatabaseException
{
    private const string ExceptionMessage =
        "The provided model is not valid.";

    internal DatabaseInvalidModel()
        : base(ExceptionMessage)
    {

    }

    internal DatabaseInvalidModel(Exception innerException)
        : base(ExceptionMessage, innerException)
    {

    }
}
