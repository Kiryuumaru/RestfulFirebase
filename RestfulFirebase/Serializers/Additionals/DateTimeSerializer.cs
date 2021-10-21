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
            var bytes = MathUtilities.ToUnsignedArbitraryBaseSystem((ulong)value.Ticks, 64);
            string base64 = "";
            foreach (var num in bytes)
            {
                base64 += StringUtilities.Base64Charset[(int)num];
            }
            return base64;
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
                var indexes = new List<uint>();
                foreach (var num in data)
                {
                    var indexOf = StringUtilities.Base64Charset.IndexOf(num);
                    indexes.Add((uint)indexOf);
                }
                var ticks = MathUtilities.ToUnsignedNormalBaseSystem(indexes.ToArray(), 64);

                return new DateTime((long)ticks);
            }
            catch
            {
                return defaultValue;
            }
        }
    }
}
