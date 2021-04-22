using System;
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

        public override DateTime Decode(string data, DateTime defaultValue = default)
        {
            if (string.IsNullOrEmpty(data)) return defaultValue;
            var dateTime = Helpers.DecodeDateTime(data, defaultValue);
            return defaultValue;
        }
    }
}
