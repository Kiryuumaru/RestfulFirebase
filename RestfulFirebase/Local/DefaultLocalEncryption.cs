using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RestfulFirebase.Local
{
    /// <inheritdoc/>
    public class DefaultLocalEncryption : ILocalEncryption
    {
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
