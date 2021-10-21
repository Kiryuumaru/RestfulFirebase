namespace RestfulFirebase.Serializers.Primitives
{
    /// <inheritdoc/>
    public class ULongSerializer : Serializer<ulong>
    {
        /// <inheritdoc/>
        public override string Serialize(ulong value)
        {
            return value.ToString();
        }

        /// <inheritdoc/>
        public override ulong Deserialize(string data, ulong defaultValue = default)
        {
            if (string.IsNullOrEmpty(data))
            {
                return defaultValue;
            }

            try
            {
                return ulong.Parse(data);
            }
            catch
            {
                return defaultValue;
            }
        }
    }
}
