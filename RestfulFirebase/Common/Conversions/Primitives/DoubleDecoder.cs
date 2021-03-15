using System;
using System.Collections.Generic;
using System.Text;
using RestfulFirebase.Common.Models;

namespace RestfulFirebase.Common.Conversions.Primitives
{
    public class DoubleDecoder : DataTypeDecoder<double>
    {
        public override Decodable CreateDerived(double value)
        {
            return new Decodable(value.ToString());
        }

        public override double ParseValue(Decodable decodable)
        {
            if (double.TryParse(decodable.Data, out double result)) return result;
            throw new Exception("Parse error");
        }
    }
}
