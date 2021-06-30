using System;
using System.Collections.Generic;
using System.Text;

namespace RestfulFirebase.Serializers.Primitives
{
    /// <inheritdoc/>
    public class LongSerializer : Serializer<long>
    {
        /// <inheritdoc/>
        public override string Serialize(long value)
        {
            return value.ToString();
        }

        /// <inheritdoc/>
        public override long Deserialize(string data)
        {
            return long.Parse(data);
        }
    }
}
