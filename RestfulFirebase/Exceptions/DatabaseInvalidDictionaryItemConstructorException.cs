using System;

namespace RestfulFirebase.Exceptions;

/// <summary>
/// Occurs when a dictionary item has no parameterless constructor.
/// </summary>
public class DatabaseInvalidDictionaryItemConstructorException : DatabaseException
{
    private const string ExceptionMessage =
        "Dictionary item with no parameterless constructor is not valid.";

    /// <summary>
    /// Creates an instance of <see cref="DatabaseInvalidDictionaryItemConstructorException"/>.
    /// </summary>
    public DatabaseInvalidDictionaryItemConstructorException()
        : base(ExceptionMessage)
    {

    }

    /// <summary>
    /// Creates an instance of <see cref="DatabaseInvalidDictionaryItemConstructorException"/> with provided <paramref name="innerException"/>.
    /// </summary>
    /// <param name="innerException">
    /// The inner exception occured.
    /// </param>
    public DatabaseInvalidDictionaryItemConstructorException(Exception innerException)
        : base(ExceptionMessage, innerException)
    {

    }
}
