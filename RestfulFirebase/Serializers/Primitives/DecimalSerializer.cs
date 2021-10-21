namespace RestfulFirebase.Serializers.Primitives
{
    /// <inheritdoc/>
    public class DecimalSerializer : Serializer<decimal>
    {
        /// <inheritdoc/>
        public override string Serialize(decimal value)
        {
            return value.ToString();
        }

        /// <inheritdoc/>
        public override decimal Deserialize(string data, decimal defaultValue = default)
        {
            if (string.IsNullOrEmpty(data))
            {
                return defaultValue;
            }

            try
            {
                return decimal.Parse(data);
            }
            catch
            {
                return defaultValue;
            }
        }
    }
}
