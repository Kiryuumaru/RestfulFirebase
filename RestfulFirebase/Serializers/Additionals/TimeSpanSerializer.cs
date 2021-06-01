using System;
using System.Collections.Generic;
using System.Text;

namespace RestfulFirebase.Serializers.Additionals
{
    public class TimeSpanSerializer : Serializer<TimeSpan>
    {
        public override string Serialize(TimeSpan value)
        {
            return value.TotalHours.ToString();
        }

        public override TimeSpan Deserialize(string data)
        {
            return TimeSpan.FromHours(double.Parse(data));
        }
    }
}
