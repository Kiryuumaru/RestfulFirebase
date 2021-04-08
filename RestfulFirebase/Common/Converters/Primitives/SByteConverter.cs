﻿using System;
using System.Collections.Generic;
using System.Text;
using RestfulFirebase.Common.Models;

namespace RestfulFirebase.Common.Converters.Primitives
{
    public class SByteConverter : DataTypeConverter<sbyte>
    {
        public override string Encode(sbyte value)
        {
            return value.ToString();
        }

        public override sbyte Decode(string data)
        {
            if (string.IsNullOrEmpty(data)) return default;
            if (sbyte.TryParse(data, out sbyte result)) return result;
            throw new Exception("Parse error");
        }
    }
}