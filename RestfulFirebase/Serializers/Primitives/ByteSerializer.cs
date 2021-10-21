namespace RestfulFirebase.Serializers.Primitives
{
    /// <inheritdoc/>
    public class ByteSerializer : Serializer<byte>
    {
        /// <inheritdoc/>
        public override string Serialize(byte value)
        {
            return value.ToString();
        }

        /// <inheritdoc/>
        public override byte Deserialize(string data, byte defaultValue = default)
        {
            if (string.IsNullOrEmpty(data))
            {
                return defaultValue;
            }

            try
            {
                return byte.Parse(data);
            }
            catch
            {
                return defaultValue;
            }
        }
    }
}
