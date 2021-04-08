using System;
using System.Collections.Generic;
using System.Text;
using RestfulFirebase.Common.Models;

namespace RestfulFirebase.Common.Converters.Additionals
{
    public class SmallDateTimeConverter : DataTypeConverter<SmallDateTime>
    {
        public override string Encode(SmallDateTime value)
        {
            return Helpers.EncodeSmallDateTime(value);
        }

        public override SmallDateTime Decode(string data)
        {
            if (string.IsNullOrEmpty(data)) return default;
            var dateTime = Helpers.DecodeSmallDateTime(data);
            if (dateTime.HasValue) return dateTime.Value;
            throw new Exception("Parse error");
        }
    }
}
