using System;
using System.Collections.Generic;
using System.Text;

namespace RestfulFirebase.Serializers.Primitives
{
    public class ByteSerializer : Serializer<byte>
    {
        public override string Serialize(byte value)
        {
            return value.ToString();
        }

        public override byte Deserialize(string data)
        {
            return byte.Parse(data);
        }
    }
}
