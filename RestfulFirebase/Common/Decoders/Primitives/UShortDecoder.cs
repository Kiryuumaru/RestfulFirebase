using System;
using System.Collections.Generic;
using System.Text;
using RestfulFirebase.Common.Models;

namespace RestfulFirebase.Common.Decoders.Primitives
{
    public class UShortDecoder : DataTypeDecoder<ushort>
    {
        public override string Encode(ushort value)
        {
            return value.ToString();
        }

        public override ushort Decode(string data)
        {
            if (string.IsNullOrEmpty(data)) return default;
            if (ushort.TryParse(data, out ushort result)) return result;
            throw new Exception("Parse error");
        }
    }
}
