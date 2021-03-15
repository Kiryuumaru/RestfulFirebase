using System;
using System.Collections.Generic;
using System.Text;
using RestfulFirebase.Common.Models;

namespace RestfulFirebase.Common.Conversions.Primitives
{
    public class FloatDecoder : DataTypeDecoder<float>
    {
        public override Decodable CreateDerived(float value)
        {
            return new Decodable(value.ToString());
        }

        public override float ParseValue(Decodable decodable)
        {
            if (float.TryParse(decodable.Data, out float result)) return result;
            throw new Exception("Parse error");
        }
    }
}
