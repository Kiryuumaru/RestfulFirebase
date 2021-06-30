using System;
using System.Collections.Generic;
using System.Text;

namespace RestfulFirebase.Serializers.Additionals
{
    /// <inheritdoc/>
    public class StringSerializer : Serializer<string>
    {
        /// <inheritdoc/>
        public override string Serialize(string value)
        {
            return value;
        }

        /// <inheritdoc/>
        public override string Deserialize(string data)
        {
            return data;
        }
    }
}
