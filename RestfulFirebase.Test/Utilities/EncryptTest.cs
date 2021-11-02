using RestfulFirebase.Local;
using RestfulFirebase.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RestfulFirebase.Test.Utilities
{
    public class EncryptTest : ILocalEncryption
    {
        private static readonly int[] EncryptionPattern = new int[] { 1, 4, 2, 3 };
        private bool isEncrypted;

        public EncryptTest(bool isEncrypted)
        {
            this.isEncrypted = isEncrypted;
        }

        public string DecryptKey(string encrypted)
        {
            if (!isEncrypted) return encrypted;
            return Cryptography.VigenereCipherDecrypt(encrypted, EncryptionPattern);
        }

        public string DecryptValue(string encrypted)
        {
            if (!isEncrypted) return encrypted;
            return Cryptography.VigenereCipherDecrypt(encrypted, EncryptionPattern);
        }

        public string EncryptKey(string key)
        {
            if (!isEncrypted) return key;
            return Cryptography.VigenereCipherEncrypt(key, EncryptionPattern);
        }

        public string EncryptValue(string value)
        {
            if (!isEncrypted) return value;
            return Cryptography.VigenereCipherEncrypt(value, EncryptionPattern);
        }
    }
}
