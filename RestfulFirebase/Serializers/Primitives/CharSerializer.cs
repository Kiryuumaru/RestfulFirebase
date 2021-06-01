using System;
using System.Collections.Generic;
using System.Text;

namespace RestfulFirebase.Serializers.Primitives
{
    public class CharSerializer : Serializer<char>
    {
        public override string Serialize(char value)
        {
            return value.ToString();
        }

        public override char Deserialize(string data)
        {
            return char.Parse(data);
        }
    }
}
