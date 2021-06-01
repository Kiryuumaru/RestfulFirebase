using System;
using System.Collections.Generic;
using System.Text;

namespace RestfulFirebase.Serializers.Primitives
{
    public class ShortSerializer : Serializer<short>
    {
        public override string Serialize(short value)
        {
            return value.ToString();
        }

        public override short Deserialize(string data)
        {
            return short.Parse(data);
        }
    }
}
