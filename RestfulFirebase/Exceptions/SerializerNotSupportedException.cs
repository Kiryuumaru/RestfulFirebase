using System;
using System.Collections.Generic;
using System.Text;

namespace RestfulFirebase.Exceptions
{
    /// <summary>
    /// Occurs when the specified serializer is not supported.
    /// </summary>
    public class SerializerNotSupportedException : SerializerException
    {
        internal SerializerNotSupportedException(Type type)
            : base("There is no supported serializer for \'" + type.Name + "\'. Register a serializer for the specified type first.")
        {

        }
    }
}
