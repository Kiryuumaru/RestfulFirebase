using System;
using System.Collections.Generic;
using System.Text;
using RestfulFirebase.Common.Models;

namespace RestfulFirebase.Common.Serializers.Primitives
{
    public class IntSerializer : Serializer<int>
    {
        public override string Serialize(int value)
        {
            return value.ToString();
        }

        public override int Deserialize(string data, int defaultValue = default)
        {
            if (string.IsNullOrEmpty(data)) return defaultValue;
            if (int.TryParse(data, out int result)) return result;
            return defaultValue;
        }
    }
}
