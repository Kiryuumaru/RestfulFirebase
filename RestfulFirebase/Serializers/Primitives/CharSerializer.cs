namespace RestfulFirebase.Serializers.Primitives
{
    /// <inheritdoc/>
    public class CharSerializer : ISerializer<char>
    {
        /// <inheritdoc/>
        public string Serialize(char value)
        {
            return value.ToString();
        }

        /// <inheritdoc/>
        public char Deserialize(string data, char defaultValue = default)
        {
            if (char.TryParse(data, out char value))
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
