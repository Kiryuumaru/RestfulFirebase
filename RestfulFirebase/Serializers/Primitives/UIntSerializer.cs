using System;
using System.Collections.Generic;
using System.Text;

namespace RestfulFirebase.Serializers.Primitives
{
    public class UIntSerializer : Serializer<uint>
    {
        public override string Serialize(uint value)
        {
            return value.ToString();
        }

        public override uint Deserialize(string data)
        {
            return uint.Parse(data);
        }
    }
}
