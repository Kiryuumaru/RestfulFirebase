using System;

namespace RestfulFirebase.Exceptions;

/// <summary>
/// Occurs when the provided node name has forbidden character. Node names cannot contain . $ # [ ] / or ASCII control characters 0-31 or 127.
/// </summary>
public class DatabaseForbiddenNodeNameCharacter : DatabaseException
{
    private const string ExceptionMessage =
        "The provided node has forbidden character.";

    /// <summary>
    /// Creates an instance of <see cref="DatabaseForbiddenNodeNameCharacter"/>.
    /// </summary>
    public DatabaseForbiddenNodeNameCharacter()
        : base(ExceptionMessage)
    {

    }

    /// <summary>
    /// Creates an instance of <see cref="DatabaseForbiddenNodeNameCharacter"/> with provided <paramref name="innerException"/>.
    /// </summary>
    /// <param name="innerException">
    /// The inner exception occured.
    /// </param>
    public DatabaseForbiddenNodeNameCharacter(Exception innerException)
        : base(ExceptionMessage, innerException)
    {

    }
}
