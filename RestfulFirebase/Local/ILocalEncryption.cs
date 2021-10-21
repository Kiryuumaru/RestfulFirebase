namespace RestfulFirebase.Local
{
    /// <summary>
    /// The local encryption declarations used for local database security.
    /// </summary>
    public interface ILocalEncryption
    {
        /// <summary>
        /// Encrypt key from all incoming keys to be save in local database.
        /// </summary>
        /// <param name="key">
        /// The key to encrypt.
        /// </param>
        /// <returns>
        /// The encrypted key to save in local database.
        /// </returns>
        string EncryptKey(string key);

        /// <summary>
        /// Encrypt value from all incoming values to be save in local database.
        /// </summary>
        /// <param name="value">
        /// The value to encrypt.
        /// </param>
        /// <returns>
        /// The encrypted value to save in local database.
        /// </returns>
        string EncryptValue(string value);

        /// <summary>
        /// Decrypt key from all incoming keys saved from the local database.
        /// </summary>
        /// <param name="encrypted">
        /// The encrypted key to decrypt.
        /// </param>
        /// <returns>
        /// The decrypted key from local database.
        /// </returns>
        string DecryptKey(string encrypted);

        /// <summary>
        /// Decrypt value from all incoming values saved from the local database.
        /// </summary>
        /// <param name="encrypted">
        /// The encrypted value to decrypt.
        /// </param>
        /// <returns>
        /// The decrypted value from local database.
        /// </returns>
        string DecryptValue(string encrypted);
    }
}
