namespace RestfulFirebase.Serializers.Additionals
{
    /// <inheritdoc/>
    public class StringSerializer : ISerializer<string>
    {
        /// <inheritdoc/>
        public string Serialize(string value)
        {
            return value;
        }

        /// <inheritdoc/>
        public string Deserialize(string data, string defaultValue = default)
        {
            return data;
        }
    }
}
