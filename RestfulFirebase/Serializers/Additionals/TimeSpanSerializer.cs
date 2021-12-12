using System;

namespace RestfulFirebase.Serializers.Additionals
{
    /// <inheritdoc/>
    public class TimeSpanSerializer : ISerializer<TimeSpan>
    {
        /// <inheritdoc/>
        public string Serialize(TimeSpan value)
        {
            return value.TotalHours.ToString();
        }

        /// <inheritdoc/>
        public TimeSpan Deserialize(string data, TimeSpan defaultValue = default)
        {
            if (double.TryParse(data, out double value))
            {
                try
                {
                    return TimeSpan.FromHours(value);
                }
                catch { }
            }

            return defaultValue;
        }
    }
}
