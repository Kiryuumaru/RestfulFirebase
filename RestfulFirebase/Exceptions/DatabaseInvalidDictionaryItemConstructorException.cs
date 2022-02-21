using System;

namespace RestfulFirebase.Exceptions;

/// <summary>
/// Occurs when a dictionary item has no parameterless constructor.
/// </summary>
public class DatabaseInvalidDictionaryItemConstructorException : DatabaseException
{
    private const string ExceptionMessage =
        "Dictionary item with no parameterless constructor is not valid.";

    internal DatabaseInvalidDictionaryItemConstructorException()
        : base(ExceptionMessage)
    {

    }

    internal DatabaseInvalidDictionaryItemConstructorException(Exception innerException)
        : base(ExceptionMessage, innerException)
    {

    }
}
