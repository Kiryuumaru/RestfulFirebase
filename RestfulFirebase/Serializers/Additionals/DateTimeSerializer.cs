using RestfulFirebase.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace RestfulFirebase.Serializers.Additionals
{
    /// <inheritdoc/>
    public class DateTimeSerializer : Serializer<DateTime>
    {
        /// <inheritdoc/>
        public override string Serialize(DateTime value)
        {
            var bytes = Utils.ToUnsignedArbitraryBaseSystem((ulong)value.Ticks, 64);
            string base64 = "";
            foreach (var num in bytes)
            {
                base64 += Utils.Base64Charset[(int)num];
            }
            return base64;
        }

        /// <inheritdoc/>
        public override DateTime Deserialize(string data)
        {
            var indexes = new List<uint>();
            foreach (var num in data)
            {
                var indexOf = Utils.Base64Charset.IndexOf(num);
                indexes.Add((uint)indexOf);
            }
            var ticks = Utils.ToUnsignedNormalBaseSystem(indexes.ToArray(), 64);

            return new DateTime((long)ticks);
        }
    }
}
