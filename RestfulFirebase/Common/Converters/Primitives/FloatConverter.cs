﻿using System;
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

        public override float Decode(string data)
        {
            if (string.IsNullOrEmpty(data)) return default;
            if (float.TryParse(data, out float result)) return result;
            throw new Exception("Parse error");
        }
    }
}