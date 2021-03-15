using System;
using System.Collections.Generic;
using System.Text;
using RestfulFirebase.Common.Models;

namespace RestfulFirebase.Common.Conversions.Primitives
{
    public class DecimalDecoder : DataTypeDecoder<decimal>
    {
        public override Decodable CreateDerived(decimal value)
        {
            return new Decodable(value.ToString());
        }

        public override decimal ParseValue(Decodable decodable)
        {
            if (decimal.TryParse(decodable.Data, out decimal result)) return result;
            throw new Exception("Parse error");
        }
    }
}
