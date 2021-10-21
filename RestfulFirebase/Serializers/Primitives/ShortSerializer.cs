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
        public override short Deserialize(string data, short defaultValue = default)
        {
            if (string.IsNullOrEmpty(data))
            {
                return defaultValue;
            }

            try
            {
                return short.Parse(data);
            }
            catch
            {
                return defaultValue;
            }
        }
    }
}
