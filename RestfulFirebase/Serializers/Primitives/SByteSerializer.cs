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
        public override sbyte Deserialize(string data, sbyte defaultValue = default)
        {
            if (string.IsNullOrEmpty(data))
            {
                return defaultValue;
            }

            try
            {
                return sbyte.Parse(data);
            }
            catch
            {
                return defaultValue;
            }
        }
    }
}
