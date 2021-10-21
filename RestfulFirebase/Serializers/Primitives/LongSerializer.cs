namespace RestfulFirebase.Serializers.Primitives
{
    /// <inheritdoc/>
    public class LongSerializer : Serializer<long>
    {
        /// <inheritdoc/>
        public override string Serialize(long value)
        {
            return value.ToString();
        }

        /// <inheritdoc/>
        public override long Deserialize(string data, long defaultValue = default)
        {
            if (string.IsNullOrEmpty(data))
            {
                return defaultValue;
            }

            try
            {
                return long.Parse(data);
            }
            catch
            {
                return defaultValue;
            }
        }
    }
}
