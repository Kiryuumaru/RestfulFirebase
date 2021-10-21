namespace RestfulFirebase.Serializers.Primitives
{
    /// <inheritdoc/>
    public class UIntSerializer : Serializer<uint>
    {
        /// <inheritdoc/>
        public override string Serialize(uint value)
        {
            return value.ToString();
        }

        /// <inheritdoc/>
        public override uint Deserialize(string data, uint defaultValue = default)
        {
            if (string.IsNullOrEmpty(data))
            {
                return defaultValue;
            }

            try
            {
                return uint.Parse(data);
            }
            catch
            {
                return defaultValue;
            }
        }
    }
}
