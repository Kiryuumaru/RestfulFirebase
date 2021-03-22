using System;
using System.Collections.Generic;
using System.Text;
using RestfulFirebase.Common.Models;

namespace RestfulFirebase.Common.Conversions.Primitives
{
    public class DecimalDecoder : DataTypeDecoder<decimal>
    {
        public override string TypeIdentifier => "decimal";

        protected override string ParseValue(decimal value)
        {
            return value.ToString();
        }

        protected override decimal ParseData(string data)
        {
            if (decimal.TryParse(data, out decimal result)) return result;
            throw new Exception("Parse error");
        }
    }
}
