using System;
using System.Collections.Generic;
using System.Text;
using RestfulFirebase.Common.Models;

namespace RestfulFirebase.Common.Conversions.Primitives
{
    public class LongDecoder : DataTypeDecoder<long>
    {
        public override ObservableProperty Parse(long value)
        {
            return ObservableProperty.CreateFromData(value.ToString());
        }

        public override long Parse(ObservableProperty decodable)
        {
            if (long.TryParse(decodable.Data, out long result)) return result;
            throw new Exception("Parse error");
        }
    }
}
