using System;
using System.Collections.Generic;
using System.Text;
using RestfulFirebase.Common.Models;

namespace RestfulFirebase.Common.Converters.Primitives
{
    public class UIntConverter : DataTypeConverter<uint>
    {
        public override string Encode(uint value)
        {
            return value.ToString();
        }

        public override uint Decode(string data)
        {
            if (string.IsNullOrEmpty(data)) return default;
            if (uint.TryParse(data, out uint result)) return result;
            throw new Exception("Parse error");
        }
    }
}
