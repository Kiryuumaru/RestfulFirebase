using System;
using System.Collections.Generic;
using System.Text;
using RestfulFirebase.Common.Models;

namespace RestfulFirebase.Common.Converters.Primitives
{
    public class FloatConverter : DataTypeConverter<float>
    {
        public override string Encode(float value)
        {
            return value.ToString();
        }

        public override float Decode(string data, float defaultValue = default)
        {
            if (string.IsNullOrEmpty(data)) return defaultValue;
            if (float.TryParse(data, out float result)) return result;
            return defaultValue;
        }
    }
}
