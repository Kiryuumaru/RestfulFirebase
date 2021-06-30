using System;
using System.Collections.Generic;
using System.Text;

namespace RestfulFirebase.Serializers.Primitives
{
    /// <inheritdoc/>
    public class ByteSerializer : Serializer<byte>
    {
        /// <inheritdoc/>
        public override string Serialize(byte value)
        {
            return value.ToString();
        }

        /// <inheritdoc/>
        public override byte Deserialize(string data)
        {
            return byte.Parse(data);
        }
    }
}
