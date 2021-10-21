namespace RestfulFirebase.Serializers.Primitives
{
    /// <inheritdoc/>
    public class CharSerializer : Serializer<char>
    {
        /// <inheritdoc/>
        public override string Serialize(char value)
        {
            return value.ToString();
        }

        /// <inheritdoc/>
        public override char Deserialize(string data, char defaultValue = default)
        {
            if (string.IsNullOrEmpty(data))
            {
                return defaultValue;
            }

            try
            {
                return char.Parse(data);
            }
            catch
            {
                return defaultValue;
            }
        }
    }
}
