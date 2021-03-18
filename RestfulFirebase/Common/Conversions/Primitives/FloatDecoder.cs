using System;
using System.Collections.Generic;
using System.Text;
using RestfulFirebase.Common.Models;

namespace RestfulFirebase.Common.Conversions.Primitives
{
    public class FloatDecoder : DataTypeDecoder<float>
    {
        public override ObservableProperty Parse(float value)
        {
            return ObservableProperty.CreateFromData(value.ToString());
        }

        public override float Parse(ObservableProperty decodable)
        {
            if (float.TryParse(decodable.Data, out float result)) return result;
            throw new Exception("Parse error");
        }
    }
}
