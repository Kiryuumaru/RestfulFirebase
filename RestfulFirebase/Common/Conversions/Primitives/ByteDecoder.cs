using System;
using System.Collections.Generic;
using System.Text;
using RestfulFirebase.Common.Models;

namespace RestfulFirebase.Common.Conversions.Primitives
{
    public class ByteDecoder : DataTypeDecoder<byte>
    {
        public override ObservablePropertyHolder.ObservableProperty CreateDerived(byte value)
        {
            return new ObservablePropertyHolder.ObservableProperty(value.ToString());
        }

        public override byte ParseValue(ObservablePropertyHolder.ObservableProperty decodable)
        {
            if (byte.TryParse(decodable.Holder.Data, out byte result)) return result;
            throw new Exception("Parse error");
        }
    }
}
