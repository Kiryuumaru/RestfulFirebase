namespace RestfulFirebase.Serializers.Additionals
{
    /// <inheritdoc/>
    public class StringSerializer : Serializer<string>
    {
        /// <inheritdoc/>
        public override string Serialize(string value)
        {
            return value;
        }

        /// <inheritdoc/>
        public override string Deserialize(string data, string defaultValue = default)
        {
            return data;
        }
    }
}
