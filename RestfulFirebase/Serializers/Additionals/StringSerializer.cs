using System;
using System.Collections.Generic;
using System.Text;

namespace RestfulFirebase.Serializers.Additionals
{
    public class StringSerializer : Serializer<string>
    {
        public override string Serialize(string value)
        {
            return value;
        }

        public override string Deserialize(string data)
        {
            return data;
        }
    }
}
