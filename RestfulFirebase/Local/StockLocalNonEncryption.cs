namespace RestfulFirebase.Local
{
    /// <summary>
    /// The provided stock <see cref="ILocalEncryption"/> implementation to be used.
    /// </summary>
    public sealed class StockLocalNonEncryption : ILocalEncryption
    {
        /// <summary>
        /// Creates new instance of <see cref="StockLocalNonEncryption"/> class.
        /// </summary>
        public StockLocalNonEncryption()
        {

        }

        /// <inheritdoc/>
        public string DecryptKey(string encrypted)
        {
            return encrypted;
        }

        /// <inheritdoc/>
        public string DecryptValue(string encrypted)
        {
            return encrypted;
        }

        /// <inheritdoc/>
        public string EncryptKey(string key)
        {
            return key;
        }

        /// <inheritdoc/>
        public string EncryptValue(string value)
        {
            return value;
        }
    }
}
