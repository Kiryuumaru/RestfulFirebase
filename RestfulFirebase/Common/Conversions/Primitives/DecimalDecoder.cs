using System;
using System.Collections.Generic;
using System.Text;
using RestfulFirebase.Common.Models;

namespace RestfulFirebase.Common.Conversions.Primitives
{
    public class DecimalDecoder : DataTypeDecoder<decimal>
    {
        public override ObservablePropertyHolder.ObservableProperty CreateDerived(decimal value)
        {
            return new ObservablePropertyHolder.ObservableProperty(value.ToString());
        }

        public override decimal ParseValue(ObservablePropertyHolder.ObservableProperty decodable)
        {
            if (decimal.TryParse(decodable.Holder.Data, out decimal result)) return result;
            throw new Exception("Parse error");
        }
    }
}
