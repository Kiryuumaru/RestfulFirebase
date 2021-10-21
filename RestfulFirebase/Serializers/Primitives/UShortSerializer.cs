namespace RestfulFirebase.Serializers.Primitives
{
    /// <inheritdoc/>
    public class UShortSerializer : Serializer<ushort>
    {
        /// <inheritdoc/>
        public override string Serialize(ushort value)
        {
            return value.ToString();
        }

        /// <inheritdoc/>
        public override ushort Deserialize(string data, ushort defaultValue = default)
        {
            if (string.IsNullOrEmpty(data))
            {
                return defaultValue;
            }

            try
            {
                return ushort.Parse(data);
            }
            catch
            {
                return defaultValue;
            }
        }
    }
}
