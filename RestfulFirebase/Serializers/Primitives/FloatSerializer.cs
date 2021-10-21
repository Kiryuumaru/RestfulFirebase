namespace RestfulFirebase.Serializers.Primitives
{
    /// <inheritdoc/>
    public class FloatSerializer : Serializer<float>
    {
        /// <inheritdoc/>
        public override string Serialize(float value)
        {
            return value.ToString();
        }

        /// <inheritdoc/>
        public override float Deserialize(string data, float defaultValue = default)
        {
            if (string.IsNullOrEmpty(data))
            {
                return defaultValue;
            }

            try
            {
                return float.Parse(data);
            }
            catch
            {
                return defaultValue;
            }
        }
    }
}
