﻿using System;
using System.Collections.Generic;
using System.Text;
using RestfulFirebase.Common.Models;

namespace RestfulFirebase.Common.Converters.Additionals
{
    public class DateTimeConverter : DataTypeConverter<DateTime>
    {
        public override string Encode(DateTime value)
        {
            return Helpers.EncodeDateTime(value);
        }

        public override DateTime Decode(string data)
        {
            if (string.IsNullOrEmpty(data)) return default;
            var dateTime = Helpers.DecodeDateTime(data);
            if (dateTime.HasValue) return dateTime.Value;
            throw new Exception("Parse error");
        }
    }
}