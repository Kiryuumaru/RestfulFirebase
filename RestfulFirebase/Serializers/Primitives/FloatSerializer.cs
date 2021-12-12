namespace RestfulFirebase.Serializers.Primitives
{
    /// <inheritdoc/>
    public class FloatSerializer : ISerializer<float>
    {
        /// <inheritdoc/>
        public string Serialize(float value)
        {
            return value.ToString();
        }

        /// <inheritdoc/>
        public float Deserialize(string data, float defaultValue = default)
        {
            if (float.TryParse(data, out float value))
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
