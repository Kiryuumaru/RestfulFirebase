using System;
using System.Collections.Generic;
using System.Text;

namespace RestfulFirebase.Serializers.Primitives
{
    /// <inheritdoc/>
    public class DoubleSerializer : Serializer<double>
    {
        /// <inheritdoc/>
        public override string Serialize(double value)
        {
            return value.ToString();
        }

        /// <inheritdoc/>
        public override double Deserialize(string data)
        {
            return double.Parse(data);
        }
    }
}
