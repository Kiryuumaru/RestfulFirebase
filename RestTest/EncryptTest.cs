using RestfulFirebase.Local;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RestTest
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
            return Utils.DecryptString(encrypted, EncryptionPattern);
        }

        public string DecryptValue(string encrypted)
        {
            if (!isEncrypted) return encrypted;
            return Utils.DecryptString(encrypted, EncryptionPattern);
        }

        public string EncryptKey(string key)
        {
            if (!isEncrypted) return key;
            return Utils.EncryptString(key, EncryptionPattern);
        }

        public string EncryptValue(string value)
        {
            if (!isEncrypted) return value;
            return Utils.EncryptString(value, EncryptionPattern);
        }
    }
}
