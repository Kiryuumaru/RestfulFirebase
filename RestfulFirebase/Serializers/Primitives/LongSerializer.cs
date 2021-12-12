namespace RestfulFirebase.Serializers.Primitives
{
    /// <inheritdoc/>
    public class LongSerializer : ISerializer<long>
    {
        /// <inheritdoc/>
        public string Serialize(long value)
        {
            return value.ToString();
        }

        /// <inheritdoc/>
        public long Deserialize(string data, long defaultValue = default)
        {
            if (long.TryParse(data, out long value))
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
