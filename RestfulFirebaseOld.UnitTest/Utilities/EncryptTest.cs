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
