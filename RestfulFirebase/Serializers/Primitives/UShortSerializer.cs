using System;
using System.Collections.Generic;
using System.Text;

namespace RestfulFirebase.Serializers.Primitives
{
    /// <inheritdoc/>
    public class UShortSerializer : Serializer<ushort>
    {
        /// <inheritdoc/>
        public override string Serialize(ushort value)
        {
            return value.ToString();
        }

        /// <inheritdoc/>
        public override ushort Deserialize(string data)
        {
            return ushort.Parse(data);
        }
    }
}
