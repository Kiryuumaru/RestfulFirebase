namespace RestfulFirebase.Serializers.Primitives
{
    /// <inheritdoc/>
    public class SByteSerializer : ISerializer<sbyte>
    {
        /// <inheritdoc/>
        public string Serialize(sbyte value)
        {
            return value.ToString();
        }

        /// <inheritdoc/>
        public sbyte Deserialize(string data, sbyte defaultValue = default)
        {
            if (sbyte.TryParse(data, out sbyte value))
            {
                return value;
            }
            else
            {
                return defaultValue;
            }
        }
    }
}
