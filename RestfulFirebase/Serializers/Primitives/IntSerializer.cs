namespace RestfulFirebase.Serializers.Primitives
{
    /// <inheritdoc/>
    public class IntSerializer : Serializer<int>
    {
        /// <inheritdoc/>
        public override string Serialize(int value)
        {
            return value.ToString();
        }

        /// <inheritdoc/>
        public override int Deserialize(string data, int defaultValue = default)
        {
            if (string.IsNullOrEmpty(data))
            {
                return defaultValue;
            }

            try
            {
                return int.Parse(data);
            }
            catch
            {
                return defaultValue;
            }
        }
    }
}
