﻿using System;
using System.Collections.Generic;
using System.Text;
using RestfulFirebase.Common.Models;

namespace RestfulFirebase.Common.Serializers.Primitives
{
    public class DecimalSerializer : Serializer<decimal>
    {
        public override string Serialize(decimal value)
        {
            return value.ToString();
        }

        public override decimal Deserialize(string data, decimal defaultValue = default)
        {
            if (string.IsNullOrEmpty(data)) return defaultValue;
            if (decimal.TryParse(data, out decimal result)) return result;
            return defaultValue;
        }
    }
}
