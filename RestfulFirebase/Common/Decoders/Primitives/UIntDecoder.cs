using System;
using System.Collections.Generic;
using System.Text;
using RestfulFirebase.Common.Models;

namespace RestfulFirebase.Common.Decoders.Primitives
{
    public class UIntDecoder : DataTypeDecoder<uint>
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
