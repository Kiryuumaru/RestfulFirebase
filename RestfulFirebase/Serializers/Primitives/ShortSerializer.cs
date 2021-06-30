using System;
using System.Collections.Generic;
using System.Text;

namespace RestfulFirebase.Serializers.Primitives
{
    /// <inheritdoc/>
    public class ShortSerializer : Serializer<short>
    {
        /// <inheritdoc/>
        public override string Serialize(short value)
        {
            return value.ToString();
        }

        /// <inheritdoc/>
        public override short Deserialize(string data)
        {
            return short.Parse(data);
        }
    }
}
