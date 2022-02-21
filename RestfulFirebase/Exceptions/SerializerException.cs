using System;

namespace RestfulFirebase.Exceptions;

/// <summary>
/// Occurs when there`s an error in serialization.
/// </summary>
public abstract class SerializerException : Exception
{
    private protected SerializerException()
    {

    }

    private protected SerializerException(Exception innerException)
        : base("An serializer error occured.", innerException)
    {

    }

    private protected SerializerException(string message)
        : base(message)
    {

    }

    private protected SerializerException(string message, Exception innerException)
        : base(message, innerException)
    {

    }
}
