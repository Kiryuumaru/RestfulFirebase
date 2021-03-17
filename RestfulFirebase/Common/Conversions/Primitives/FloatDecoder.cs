using System;
using System.Collections.Generic;
using System.Text;
using RestfulFirebase.Common.Models;

namespace RestfulFirebase.Common.Conversions.Primitives
{
    public class FloatDecoder : DataTypeDecoder<float>
    {
        public override ObservablePropertyHolder.ObservableProperty CreateDerived(float value)
        {
            return new ObservablePropertyHolder.ObservableProperty(value.ToString());
        }

        public override float ParseValue(ObservablePropertyHolder.ObservableProperty decodable)
        {
            if (float.TryParse(decodable.Holder.Data, out float result)) return result;
            throw new Exception("Parse error");
        }
    }
}
