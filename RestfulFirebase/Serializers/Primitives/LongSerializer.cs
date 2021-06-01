using System;
using System.Collections.Generic;
using System.Text;

namespace RestfulFirebase.Serializers.Primitives
{
    public class LongSerializer : Serializer<long>
    {
        public override string Serialize(long value)
        {
            return value.ToString();
        }

        public override long Deserialize(string data)
        {
            return long.Parse(data);
        }
    }
}
