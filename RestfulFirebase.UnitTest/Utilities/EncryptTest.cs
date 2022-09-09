using RestfulFirebase.Local;
using RestfulFirebase.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RestfulFirebase.UnitTest.Utilities
{
    public class EncryptTest : ILocalEncryption
    {
        private static readonly int[] EncryptionPattern = new int[] { 1, 4, 2, 3 };
        private readonly bool isEncrypted;

        public EncryptTest(bool isEncrypted)
        {
            this.isEncrypted = isEncrypted;
        }

        public string? Decrypt(string? value)
        {
            if (!isEncrypted) return value;
            return Cryptography.VigenereCipherDecrypt(value, EncryptionPattern);
        }

        public string? Encrypt(string? value)
        {
            if (!isEncrypted) return value;
            return Cryptography.VigenereCipherEncrypt(value, EncryptionPattern);
        }
    }
}
