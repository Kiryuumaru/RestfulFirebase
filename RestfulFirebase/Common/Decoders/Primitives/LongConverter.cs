﻿using System;
using System.Collections.Generic;
using System.Text;
using RestfulFirebase.Common.Models;

namespace RestfulFirebase.Common.Converters.Primitives
{
    public class LongConverter : DataTypeConverter<long>
    {
        public override string Encode(long value)
        {
            return value.ToString();
        }

        public override long Decode(string data)
        {
            if (string.IsNullOrEmpty(data)) return default;
            if (long.TryParse(data, out long result)) return result;
            throw new Exception("Parse error");
        }
    }
}