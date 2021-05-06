using System;
using System.Collections.Generic;
using System.Text;
using RestfulFirebase.Common.Models;

namespace RestfulFirebase.Common.Serializers.Additionals
{
    public class SmallDateTimeSerializer : Serializer<SmallDateTime>
    {
        public override string Serialize(SmallDateTime value)
        {
            return Helpers.EncodeSmallDateTime(value);
        }

        public override SmallDateTime Deserialize(string data, SmallDateTime defaultValue = default)
        {
            if (string.IsNullOrEmpty(data)) return defaultValue;
            var dateTime = Helpers.DecodeSmallDateTime(data);
            if (dateTime.HasValue) return dateTime.Value;
            return defaultValue;
        }
    }
}
