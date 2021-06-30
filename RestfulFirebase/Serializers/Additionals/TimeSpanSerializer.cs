using System;
using System.Collections.Generic;
using System.Text;

namespace RestfulFirebase.Serializers.Additionals
{
    /// <inheritdoc/>
    public class TimeSpanSerializer : Serializer<TimeSpan>
    {
        /// <inheritdoc/>
        public override string Serialize(TimeSpan value)
        {
            return value.TotalHours.ToString();
        }

        /// <inheritdoc/>
        public override TimeSpan Deserialize(string data)
        {
            return TimeSpan.FromHours(double.Parse(data));
        }
    }
}
