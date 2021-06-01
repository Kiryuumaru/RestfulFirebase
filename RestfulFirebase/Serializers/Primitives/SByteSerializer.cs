using System;
using System.Collections.Generic;
using System.Text;

namespace RestfulFirebase.Serializers.Primitives
{
    public class SByteSerializer : Serializer<sbyte>
    {
        public override string Serialize(sbyte value)
        {
            return value.ToString();
        }

        public override sbyte Deserialize(string data)
        {
            return sbyte.Parse(data);
        }
    }
}
