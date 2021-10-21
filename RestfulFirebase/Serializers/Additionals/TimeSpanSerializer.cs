using System;

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
        public override TimeSpan Deserialize(string data, TimeSpan defaultValue = default)
        {
            if (string.IsNullOrEmpty(data))
            {
                return defaultValue;
            }

            try
            {
                return TimeSpan.FromHours(double.Parse(data));
            }
            catch
            {
                return defaultValue;
            }
        }
    }
}
