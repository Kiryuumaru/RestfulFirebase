using System;
using System.Collections.Generic;
using System.Text;
using RestfulFirebase.Common.Models;

namespace RestfulFirebase.Common.Conversions.Primitives
{
    public class DecimalDecoder : DataTypeDecoder<decimal>
    {
        public override ObservableProperty Parse(decimal value)
        {
            return ObservableProperty.CreateFromData(value.ToString());
        }

        public override decimal Parse(ObservableProperty decodable)
        {
            if (decimal.TryParse(decodable.Data, out decimal result)) return result;
            throw new Exception("Parse error");
        }
    }
}
