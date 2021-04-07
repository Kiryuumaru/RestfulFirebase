using System;
using System.Collections.Generic;
using System.Text;
using RestfulFirebase.Common.Models;

namespace RestfulFirebase.Common.Decoders.Primitives
{
    public class ByteDecoder : DataTypeDecoder<byte>
    {
        public override string Encode(byte value)
        {
            return value.ToString();
        }

        public override byte Decode(string data)
        {
            if (string.IsNullOrEmpty(data)) return default;
            if (byte.TryParse(data, out byte result)) return result;
            throw new Exception("Parse error");
        }
    }
}
