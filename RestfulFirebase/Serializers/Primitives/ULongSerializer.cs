using System;
using System.Collections.Generic;
using System.Text;

namespace RestfulFirebase.Serializers.Primitives
{
    /// <inheritdoc/>
    public class ULongSerializer : Serializer<ulong>
    {
        /// <inheritdoc/>
        public override string Serialize(ulong value)
        {
            return value.ToString();
        }

        /// <inheritdoc/>
        public override ulong Deserialize(string data)
        {
            return ulong.Parse(data);
        }
    }
}
