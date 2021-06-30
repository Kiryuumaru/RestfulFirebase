using System;
using System.Collections.Generic;
using System.Text;

namespace RestfulFirebase.Serializers.Primitives
{
    /// <inheritdoc/>
    public class CharSerializer : Serializer<char>
    {
        /// <inheritdoc/>
        public override string Serialize(char value)
        {
            return value.ToString();
        }

        /// <inheritdoc/>
        public override char Deserialize(string data)
        {
            return char.Parse(data);
        }
    }
}
