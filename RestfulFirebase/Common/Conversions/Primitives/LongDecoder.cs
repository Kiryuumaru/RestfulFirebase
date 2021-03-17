using System;
using System.Collections.Generic;
using System.Text;
using RestfulFirebase.Common.Models;

namespace RestfulFirebase.Common.Conversions.Primitives
{
    public class LongDecoder : DataTypeDecoder<long>
    {
        public override ObservablePropertyHolder.ObservableProperty CreateDerived(long value)
        {
            return new ObservablePropertyHolder.ObservableProperty(value.ToString());
        }

        public override long ParseValue(ObservablePropertyHolder.ObservableProperty decodable)
        {
            if (long.TryParse(decodable.Holder.Data, out long result)) return result;
            throw new Exception("Parse error");
        }
    }
}
