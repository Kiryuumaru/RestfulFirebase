using System;
using System.Collections.Generic;
using System.Text;
using RestfulFirebase.Common.Models;

namespace RestfulFirebase.Common.Conversions.Primitives
{
    public class DoubleDecoder : DataTypeDecoder<double>
    {
        public override ObservableProperty Parse(double value)
        {
            return ObservableProperty.CreateFromData(value.ToString());
        }

        public override double Parse(ObservableProperty decodable)
        {
            if (double.TryParse(decodable.Data, out double result)) return result;
            throw new Exception("Parse error");
        }
    }
}
