﻿using System;
using System.Collections.Generic;
using System.Text;
using RestfulFirebase.Common.Models;

namespace RestfulFirebase.Common.Conversions.Additionals
{
    public class TimeSpanDecoder : DataTypeDecoder<TimeSpan>
    {
        public override string Encode(TimeSpan value)
        {
            return value.TotalHours.ToString();
        }

        public override TimeSpan Decode(string data)
        {
            if (double.TryParse(data, out double result)) return TimeSpan.FromHours(result);
            throw new Exception("Parse error");
        }
    }
}
