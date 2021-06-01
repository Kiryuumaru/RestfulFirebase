using System;
using System.Collections.Generic;
using System.Text;

namespace RestfulFirebase.Serializers.Primitives
{
    public class IntSerializer : Serializer<int>
    {
        public override string Serialize(int value)
        {
            return value.ToString();
        }

        public override int Deserialize(string data)
        {
            return int.Parse(data);
        }
    }
}
