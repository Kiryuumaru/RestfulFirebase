using System;
using System.Collections.Generic;
using System.Text;

namespace RestfulFirebase.Serializers.Primitives
{
    public class ULongSerializer : Serializer<ulong>
    {
        public override string Serialize(ulong value)
        {
            return value.ToString();
        }

        public override ulong Deserialize(string data)
        {
            return ulong.Parse(data);
        }
    }
}
