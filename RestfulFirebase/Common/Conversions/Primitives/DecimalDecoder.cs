using System;
using System.Collections.Generic;
using System.Text;
using RestfulFirebase.Common.Models;

namespace RestfulFirebase.Common.Conversions.Primitives
{
    public class DecimalDecoder : DataTypeDecoder<decimal>
    {
        public override string Encode(decimal value)
        {
            return value.ToString();
        }

        public override decimal Decode(string data)
        {
            if (decimal.TryParse(data, out decimal result)) return result;
            throw new Exception("Parse error");
        }
    }
}
