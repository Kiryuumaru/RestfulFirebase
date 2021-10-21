namespace RestfulFirebase.Serializers.Primitives
{
    /// <inheritdoc/>
    public class DoubleSerializer : Serializer<double>
    {
        /// <inheritdoc/>
        public override string Serialize(double value)
        {
            return value.ToString();
        }

        /// <inheritdoc/>
        public override double Deserialize(string data, double defaultValue = default)
        {
            if (string.IsNullOrEmpty(data))
            {
                return defaultValue;
            }

            try
            {
                return double.Parse(data);
            }
            catch
            {
                return defaultValue;
            }
        }
    }
}
