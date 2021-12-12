namespace RestfulFirebase.Serializers.Primitives
{
    /// <inheritdoc/>
    public class ULongSerializer : ISerializer<ulong>
    {
        /// <inheritdoc/>
        public string Serialize(ulong value)
        {
            return value.ToString();
        }

        /// <inheritdoc/>
        public ulong Deserialize(string data, ulong defaultValue = default)
        {
            if (ulong.TryParse(data, out ulong value))
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
