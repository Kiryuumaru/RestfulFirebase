using System;
using System.Collections.Generic;
using System.Text;
using RestfulFirebase.Common.Models;

namespace RestfulFirebase.Common.Conversions.Primitives
{
    public class ByteDecoder : DataTypeDecoder<byte>
    {
        public override Decodable CreateDerived(byte value)
        {
            return new Decodable(value.ToString());
        }

        public override byte ParseValue(Decodable decodable)
        {
            if (byte.TryParse(decodable.Data, out byte result)) return result;
            throw new Exception("Parse error");
        }
    }
}
