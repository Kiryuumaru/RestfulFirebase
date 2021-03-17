using System;
using System.Collections.Generic;
using System.Text;
using RestfulFirebase.Common.Models;

namespace RestfulFirebase.Common.Conversions.Primitives
{
    public class DoubleDecoder : DataTypeDecoder<double>
    {
        public override ObservablePropertyHolder.ObservableProperty CreateDerived(double value)
        {
            return new ObservablePropertyHolder.ObservableProperty(value.ToString());
        }

        public override double ParseValue(ObservablePropertyHolder.ObservableProperty decodable)
        {
            if (double.TryParse(decodable.Holder.Data, out double result)) return result;
            throw new Exception("Parse error");
        }
    }
}
