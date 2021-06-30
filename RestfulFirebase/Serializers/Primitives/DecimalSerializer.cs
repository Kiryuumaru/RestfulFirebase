using System;
using System.Collections.Generic;
using System.Text;

namespace RestfulFirebase.Serializers.Primitives
{
    /// <inheritdoc/>
    public class DecimalSerializer : Serializer<decimal>
    {
        /// <inheritdoc/>
        public override string Serialize(decimal value)
        {
            return value.ToString();
        }

        /// <inheritdoc/>
        public override decimal Deserialize(string data)
        {
            return decimal.Parse(data);
        }
    }
}
