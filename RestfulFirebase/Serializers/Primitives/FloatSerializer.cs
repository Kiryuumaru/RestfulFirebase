using System;
using System.Collections.Generic;
using System.Text;

namespace RestfulFirebase.Serializers.Primitives
{
    public class FloatSerializer : Serializer<float>
    {
        public override string Serialize(float value)
        {
            return value.ToString();
        }

        public override float Deserialize(string data)
        {
            return float.Parse(data);
        }
    }
}
