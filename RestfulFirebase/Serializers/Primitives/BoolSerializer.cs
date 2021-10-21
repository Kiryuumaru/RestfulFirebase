namespace RestfulFirebase.Serializers.Primitives
{
    /// <inheritdoc/>
    public class BoolSerializer : Serializer<bool>
    {
        /// <inheritdoc/>
        public override string Serialize(bool value)
        {
            return value ? "1" : "0";
        }

        /// <inheritdoc/>
        public override bool Deserialize(string data, bool defaultValue = default)
        {
            if (string.IsNullOrEmpty(data))
            {
                return defaultValue;
            }

            try
            {
                return data.Equals("1");
            }
            catch
            {
                return defaultValue;
            }
        }
    }
}
