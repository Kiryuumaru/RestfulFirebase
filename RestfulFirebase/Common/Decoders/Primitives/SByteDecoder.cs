using System;
using System.Collections.Generic;
using System.Text;
using RestfulFirebase.Common.Models;

namespace RestfulFirebase.Common.Decoders.Primitives
{
    public class SByteDecoder : DataTypeDecoder<sbyte>
    {
        public override string Encode(sbyte value)
        {
            return value.ToString();
        }

        public override sbyte Decode(string data)
        {
            if (string.IsNullOrEmpty(data)) return default;
            if (sbyte.TryParse(data, out sbyte result)) return result;
            throw new Exception("Parse error");
        }
    }
}
