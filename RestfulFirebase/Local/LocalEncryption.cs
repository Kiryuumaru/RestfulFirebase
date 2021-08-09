using RestfulFirebase.Extensions;
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
    /// The stock provided local encryption implementation to be optionally used for local encryption.
    /// </summary>
    public class LocalEncryption : ILocalEncryption
    {
        private static readonly int[] EncryptionPattern = new int[] { 1, 4, 2, 3 };

        /// <inheritdoc/>
        public string DecryptKey(string encrypted)
        {
            return Utils.DecryptString(encrypted, EncryptionPattern);
        }

        /// <inheritdoc/>
        public string DecryptValue(string encrypted)
        {
            return Utils.DecryptString(encrypted, EncryptionPattern);
        }

        /// <inheritdoc/>
        public string EncryptKey(string key)
        {
            return Utils.EncryptString(key, EncryptionPattern);
        }

        /// <inheritdoc/>
        public string EncryptValue(string value)
        {
            return Utils.EncryptString(value, EncryptionPattern);
        }
    }
}
