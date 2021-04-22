using System;
using System.Collections.Generic;
using System.Text;
using RestfulFirebase.Common.Models;

namespace RestfulFirebase.Common.Converters.Primitives
{
    public class DecimalConverter : DataTypeConverter<decimal>
    {
        public override string Encode(decimal value)
        {
            return value.ToString();
        }

        public override decimal Decode(string data, decimal defaultValue = default)
        {
            if (string.IsNullOrEmpty(data)) return defaultValue;
            if (decimal.TryParse(data, out decimal result)) return result;
            return defaultValue;
        }
    }
}
