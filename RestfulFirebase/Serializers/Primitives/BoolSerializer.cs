using System;
using System.Collections.Generic;
using System.Text;

namespace RestfulFirebase.Serializers.Primitives
{
    public class BoolSerializer : Serializer<bool>
    {
        public override string Serialize(bool value)
        {
            return value ? "1" : "0";
        }

        public override bool Deserialize(string data)
        {
            return data.Equals("1");
        }
    }
}
