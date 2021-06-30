using System;
using System.Collections.Generic;
using System.Text;

namespace RestfulFirebase.Serializers.Primitives
{
    /// <inheritdoc/>
    public class FloatSerializer : Serializer<float>
    {
        /// <inheritdoc/>
        public override string Serialize(float value)
        {
            return value.ToString();
        }

        /// <inheritdoc/>
        public override float Deserialize(string data)
        {
            return float.Parse(data);
        }
    }
}
