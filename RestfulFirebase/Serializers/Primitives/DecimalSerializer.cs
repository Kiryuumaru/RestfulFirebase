using System;
using System.Collections.Generic;
using System.Text;

namespace RestfulFirebase.Serializers.Primitives
{
    public class DecimalSerializer : Serializer<decimal>
    {
        public override string Serialize(decimal value)
        {
            return value.ToString();
        }

        public override decimal Deserialize(string data)
        {
            return decimal.Parse(data);
        }
    }
}
