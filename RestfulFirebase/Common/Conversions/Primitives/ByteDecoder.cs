using System;
using System.Collections.Generic;
using System.Text;
using RestfulFirebase.Common.Models;

namespace RestfulFirebase.Common.Conversions.Primitives
{
    public class ByteDecoder : DataTypeDecoder<byte>
    {
        public override ObservableProperty Parse(byte value)
        {
            return ObservableProperty.CreateFromData(value.ToString());
        }

        public override byte Parse(ObservableProperty decodable)
        {
            if (byte.TryParse(decodable.Data, out byte result)) return result;
            throw new Exception("Parse error");
        }
    }
}
