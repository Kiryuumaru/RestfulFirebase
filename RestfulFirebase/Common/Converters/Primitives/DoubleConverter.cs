﻿using System;
using System.Collections.Generic;
using System.Text;
using RestfulFirebase.Common.Models;

namespace RestfulFirebase.Common.Converters.Primitives
{
    public class DoubleConverter : DataTypeConverter<double>
    {
        public override string Encode(double value)
        {
            return value.ToString();
        }

        public override double Decode(string data)
        {
            if (string.IsNullOrEmpty(data)) return default;
            if (double.TryParse(data, out double result)) return result;
            throw new Exception("Parse error");
        }
    }
}