using System;
using System.Collections.Generic;
using System.Text;
using RestfulFirebase.Common.Models;

namespace RestfulFirebase.Common.Conversions.Primitives
{
    public class FloatDecoder : DataTypeDecoder<float>
    {
        public override string Encode(float value)
        {
            return value.ToString();
        }

        public override float Decode(string data)
        {
            if (float.TryParse(data, out float result)) return result;
            throw new Exception("Parse error");
        }
    }
}
