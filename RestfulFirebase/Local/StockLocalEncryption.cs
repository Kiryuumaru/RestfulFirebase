using RestfulFirebase.Utilities;
using RestfulFirebase.Local;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RestfulFirebase.Local
{
    /// <summary>
    /// The provided stock <see cref="ILocalEncryption"/> implementation to be used.
    /// </summary>
    public sealed class StockLocalEncryption : ILocalEncryption
    {
        private static readonly int[] EncryptionPattern = new int[] { 1, 4, 2, 3 };

        /// <summary>
        /// Creates new instance of <see cref="StockLocalEncryption"/> class.
        /// </summary>
        public StockLocalEncryption()
        {

        }

        /// <inheritdoc/>
        public string DecryptKey(string encrypted)
        {
            return Cryptography.VigenereCipherDecrypt(encrypted, EncryptionPattern);
        }

        /// <inheritdoc/>
        public string DecryptValue(string encrypted)
        {
            return Cryptography.VigenereCipherDecrypt(encrypted, EncryptionPattern);
        }

        /// <inheritdoc/>
        public string EncryptKey(string key)
        {
            return Cryptography.VigenereCipherEncrypt(key, EncryptionPattern);
        }

        /// <inheritdoc/>
        public string EncryptValue(string value)
        {
            return Cryptography.VigenereCipherEncrypt(value, EncryptionPattern);
        }
    }
}
