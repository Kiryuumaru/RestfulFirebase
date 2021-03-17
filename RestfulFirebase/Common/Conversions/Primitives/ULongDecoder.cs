using System;
using System.Collections.Generic;
using System.Text;
using RestfulFirebase.Common.Models;

namespace RestfulFirebase.Common.Conversions.Primitives
{
    public class ULongDecoder : DataTypeDecoder<ulong>
    {
        public override ObservablePropertyHolder.ObservableProperty CreateDerived(ulong value)
        {
            return new ObservablePropertyHolder.ObservableProperty(value.ToString());
        }

        public override ulong ParseValue(ObservablePropertyHolder.ObservableProperty decodable)
        {
            if (ulong.TryParse(decodable.Holder.Data, out ulong result)) return result;
            throw new Exception("Parse error");
        }
    }
}
