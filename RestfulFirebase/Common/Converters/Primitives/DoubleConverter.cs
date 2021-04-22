using System;
using System.Collections.Generic;
using System.Text;
using RestfulFirebase.Common.Models;

namespace RestfulFirebase.Common.Converters.Primitives
{
    public class DoubleConverter : DataTypeConverter<double>
    {
        public override string Encode(double value)
        {
            return value.ToString();
        }

        public override double Decode(string data, double defaultValue = default)
        {
            if (string.IsNullOrEmpty(data)) return defaultValue;
            if (double.TryParse(data, out double result)) return result;
            return defaultValue;
        }
    }
}
