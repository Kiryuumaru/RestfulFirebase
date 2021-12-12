namespace RestfulFirebase.Serializers.Primitives
{
    /// <inheritdoc/>
    public class IntSerializer : ISerializer<int>
    {
        /// <inheritdoc/>
        public string Serialize(int value)
        {
            return value.ToString();
        }

        /// <inheritdoc/>
        public int Deserialize(string data, int defaultValue = default)
        {
            if (int.TryParse(data, out int value))
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
