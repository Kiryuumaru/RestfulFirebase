using System;

namespace RestfulFirebase.Exceptions;

/// <summary>
/// Occurs when the provided node name has forbidden character. Node names cannot contain . $ # [ ] / or ASCII control characters 0-31 or 127.
/// </summary>
public class DatabaseForbiddenNodeNameCharacter : DatabaseException
{
    private const string ExceptionMessage =
        "The provided node has forbidden character.";

    internal DatabaseForbiddenNodeNameCharacter()
        : base(ExceptionMessage)
    {

    }

    internal DatabaseForbiddenNodeNameCharacter(Exception innerException)
        : base(ExceptionMessage, innerException)
    {

    }
}
