using System;
using System.Collections.Generic;
using System.Text;
using RestfulFirebase.Common.Models;

namespace RestfulFirebase.Common.Converters.Primitives
{
    public class ByteConverter : DataTypeConverter<byte>
    {
        public override string Encode(byte value)
        {
            return value.ToString();
        }

        public override byte Decode(string data, byte defaultValue = default)
        {
            if (string.IsNullOrEmpty(data)) return defaultValue;
            if (byte.TryParse(data, out byte result)) return result;
            return defaultValue;
        }
    }
}
