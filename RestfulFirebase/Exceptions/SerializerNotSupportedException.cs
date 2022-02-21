using System;

namespace RestfulFirebase.Exceptions;

/// <summary>
/// Occurs when the specified serializer is not supported.
/// </summary>
public class SerializerNotSupportedException : SerializerException
{
    internal SerializerNotSupportedException(Type type)
        : base("There is no supported serializer for \'" + type.Name + "\'. Register a serializer for the specified type first.")
    {

    }

    internal SerializerNotSupportedException(string fullname)
        : base("There is no supported serializer for \'" + fullname + "\'. Register a serializer for the specified type first.")
    {

    }
}
