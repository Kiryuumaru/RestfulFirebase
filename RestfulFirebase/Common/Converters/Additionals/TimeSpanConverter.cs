using System;
using System.Collections.Generic;
using System.Text;
using RestfulFirebase.Common.Models;

namespace RestfulFirebase.Common.Converters.Additionals
{
    public class TimeSpanConverter : DataTypeConverter<TimeSpan>
    {
        public override string Encode(TimeSpan value)
        {
            return value.TotalHours.ToString();
        }

        public override TimeSpan Decode(string data, TimeSpan defaultValue = default)
        {
            if (string.IsNullOrEmpty(data)) return defaultValue;
            if (double.TryParse(data, out double result)) return TimeSpan.FromHours(result);
            return defaultValue;
        }
    }
}
