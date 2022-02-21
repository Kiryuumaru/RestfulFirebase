using System;

namespace RestfulFirebase.Exceptions;

/// <summary>
/// Occurs when the specified serializer is not supported.
/// </summary>
public class SerializerInvalidValueProvided : SerializerException
{
    private const string ExceptionMessage =
        "The provided value is invalid serialized data.";

    internal SerializerInvalidValueProvided()
        : base(ExceptionMessage)
    {

    }

    internal SerializerInvalidValueProvided(Exception innerException)
        : base(ExceptionMessage, innerException)
    {

    }
}
