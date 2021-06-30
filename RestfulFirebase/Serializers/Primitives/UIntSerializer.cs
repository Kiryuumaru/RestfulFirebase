using System;
using System.Collections.Generic;
using System.Text;

namespace RestfulFirebase.Serializers.Primitives
{
    /// <inheritdoc/>
    public class UIntSerializer : Serializer<uint>
    {
        /// <inheritdoc/>
        public override string Serialize(uint value)
        {
            return value.ToString();
        }

        /// <inheritdoc/>
        public override uint Deserialize(string data)
        {
            return uint.Parse(data);
        }
    }
}
