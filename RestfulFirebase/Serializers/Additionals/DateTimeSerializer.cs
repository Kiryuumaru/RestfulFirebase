using RestfulFirebase.Utilities;
using System;
using System.Collections.Generic;

namespace RestfulFirebase.Serializers.Additionals
{
    /// <inheritdoc/>
    public class DateTimeSerializer : Serializer<DateTime>
    {
        /// <inheritdoc/>
        public override string Serialize(DateTime value)
        {
            return StringUtilities.CompressNumber(value.Ticks);
        }

        /// <inheritdoc/>
        public override DateTime Deserialize(string data, DateTime defaultValue = default)
        {
            if (string.IsNullOrEmpty(data))
            {
                return defaultValue;
            }

            try
            {
                return new DateTime(StringUtilities.ExtractNumber(data));
            }
            catch
            {
                return defaultValue;
            }
        }
    }
}
