using System;
using System.Collections.Generic;
using System.Text;

namespace RestfulFirebase.Serializers.Primitives
{
    /// <inheritdoc/>
    public class BoolSerializer : Serializer<bool>
    {
        /// <inheritdoc/>
        public override string Serialize(bool value)
        {
            return value ? "1" : "0";
        }

        /// <inheritdoc/>
        public override bool Deserialize(string data)
        {
            return data.Equals("1");
        }
    }
}
