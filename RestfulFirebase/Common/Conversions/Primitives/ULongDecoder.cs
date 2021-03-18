using System;
using System.Collections.Generic;
using System.Text;
using RestfulFirebase.Common.Models;

namespace RestfulFirebase.Common.Conversions.Primitives
{
    public class ULongDecoder : DataTypeDecoder<ulong>
    {
        public override ObservableProperty Parse(ulong value)
        {
            return ObservableProperty.CreateFromData(value.ToString());
        }

        public override ulong Parse(ObservableProperty decodable)
        {
            if (ulong.TryParse(decodable.Data, out ulong result)) return result;
            throw new Exception("Parse error");
        }
    }
}
