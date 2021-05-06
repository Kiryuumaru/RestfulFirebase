using System;
using System.Collections.Generic;
using System.Text;
using RestfulFirebase.Common.Models;

namespace RestfulFirebase.Common.Serializers.Additionals
{
    public class DateTimeSerializer : Serializer<DateTime>
    {
        public override string Serialize(DateTime value)
        {
            return Helpers.EncodeDateTime(value);
        }

        public override DateTime Deserialize(string data, DateTime defaultValue = default)
        {
            if (string.IsNullOrEmpty(data)) return defaultValue;
            var dateTime = Helpers.DecodeDateTime(data, defaultValue);
            return defaultValue;
        }
    }
}
