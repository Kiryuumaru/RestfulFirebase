using System;
using System.Collections.Generic;
using System.Text;

namespace RestfulFirebase.Serializers.Primitives
{
    public class DoubleSerializer : Serializer<double>
    {
        public override string Serialize(double value)
        {
            return value.ToString();
        }

        public override double Deserialize(string data)
        {
            return double.Parse(data);
        }
    }
}
