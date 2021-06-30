using System;
using System.Collections.Generic;
using System.Text;

namespace RestfulFirebase.Serializers.Primitives
{
    /// <inheritdoc/>
    public class SByteSerializer : Serializer<sbyte>
    {
        /// <inheritdoc/>
        public override string Serialize(sbyte value)
        {
            return value.ToString();
        }

        /// <inheritdoc/>
        public override sbyte Deserialize(string data)
        {
            return sbyte.Parse(data);
        }
    }
}
