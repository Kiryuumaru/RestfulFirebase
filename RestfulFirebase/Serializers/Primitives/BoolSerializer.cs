namespace RestfulFirebase.Serializers.Primitives
{
    /// <inheritdoc/>
    public class BoolSerializer : ISerializer<bool>
    {
        /// <inheritdoc/>
        public string Serialize(bool value)
        {
            return value ? "1" : "0";
        }

        /// <inheritdoc/>
        public bool Deserialize(string data, bool defaultValue = default)
        {
            if (data == "1")
            {
                return true;
            }
            else if (data == "0")
            {
                return false;
            }
            else
            {
                return defaultValue;
            }
        }
    }
}
