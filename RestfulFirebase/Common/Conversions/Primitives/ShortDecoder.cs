using System;
using System.Collections.Generic;
using System.Text;
using RestfulFirebase.Common.Models;

namespace RestfulFirebase.Common.Conversions.Primitives
{
    public class ShortDecoder : DataTypeDecoder<short>
    {
        public override ObservablePropertyHolder.ObservableProperty CreateDerived(short value)
        {
            return new ObservablePropertyHolder.ObservableProperty(value.ToString());
        }

        public override short ParseValue(ObservablePropertyHolder.ObservableProperty decodable)
        {
            if (short.TryParse(decodable.Holder.Data, out short result)) return result;
            throw new Exception("Parse error");
        }
    }
}
