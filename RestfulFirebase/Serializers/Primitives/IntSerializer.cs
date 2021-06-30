using System;
using System.Collections.Generic;
using System.Text;

namespace RestfulFirebase.Serializers.Primitives
{
    /// <inheritdoc/>
    public class IntSerializer : Serializer<int>
    {
        /// <inheritdoc/>
        public override string Serialize(int value)
        {
            return value.ToString();
        }

        /// <inheritdoc/>
        public override int Deserialize(string data)
        {
            return int.Parse(data);
        }
    }
}
