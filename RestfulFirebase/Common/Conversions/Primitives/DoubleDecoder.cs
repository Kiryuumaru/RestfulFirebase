using System;
using System.Collections.Generic;
using System.Text;
using RestfulFirebase.Common.Models;

namespace RestfulFirebase.Common.Conversions.Primitives
{
    public class DoubleDecoder : DataTypeDecoder<double>
    {
        public override string TypeIdentifier => "double";

        protected override string ParseValue(double value)
        {
            return value.ToString();
        }

        protected override double ParseData(string data)
        {
            if (double.TryParse(data, out double result)) return result;
            throw new Exception("Parse error");
        }
    }
}
