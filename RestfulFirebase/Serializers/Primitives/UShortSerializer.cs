using System;
using System.Collections.Generic;
using System.Text;

namespace RestfulFirebase.Serializers.Primitives
{
    public class UShortSerializer : Serializer<ushort>
    {
        public override string Serialize(ushort value)
        {
            return value.ToString();
        }

        public override ushort Deserialize(string data)
        {
            return ushort.Parse(data);
        }
    }
}
