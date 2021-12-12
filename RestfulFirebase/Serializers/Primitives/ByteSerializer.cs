namespace RestfulFirebase.Serializers.Primitives
{
    /// <inheritdoc/>
    public class ByteSerializer : ISerializer<byte>
    {
        /// <inheritdoc/>
        public string Serialize(byte value)
        {
            return value.ToString();
        }

        /// <inheritdoc/>
        public byte Deserialize(string data, byte defaultValue = default)
        {
            if (byte.TryParse(data, out byte value))
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
