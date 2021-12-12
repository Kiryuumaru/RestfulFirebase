namespace RestfulFirebase.Serializers.Primitives
{
    /// <inheritdoc/>
    public class DecimalSerializer : ISerializer<decimal>
    {
        /// <inheritdoc/>
        public string Serialize(decimal value)
        {
            return value.ToString();
        }

        /// <inheritdoc/>
        public decimal Deserialize(string data, decimal defaultValue = default)
        {
            if (decimal.TryParse(data, out decimal value))
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
