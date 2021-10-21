using System.Text;

namespace RestfulFirebase.Utilities
{
    /// <summary>
    /// Provides cryptography algorithms for unicode strings.
    /// </summary>
    public static class Cryptography
    {
        /// <summary>
        /// Encrypts unicode string by using a series of interwoven Caesar ciphers, based on the <paramref name="pattern"/> parameter.
        /// </summary>
        /// <param name="value">
        /// The value to encrypt.
        /// </param>
        /// <param name="pattern">
        /// The pattern on encryption to use.
        /// </param>
        /// <returns>
        /// The encrypted representation of the <paramref name="value"/> parameter.
        /// </returns>
        public static string VigenereCipherEncrypt(string value, params int[] pattern)
        {
            if (value == null)
            {
                return null;
            }
            if (pattern?.Length == 0)
            {
                return value;
            }

            StringBuilder builder = new StringBuilder();
            int patternIndex = 0;
            for (int i = 0; i < value.Length; i++)
            {
                char pos = (char)(value[i] + pattern[patternIndex]);
                builder.Append(char.MaxValue < pos ? (char)(pos - char.MaxValue) : pos);
                patternIndex = (patternIndex + 1) >= pattern.Length ? 0 : patternIndex + 1;
            }
            return builder.ToString();
        }

        /// <summary>
        /// Decrypts unicode string by using a series of interwoven Caesar ciphers, based on the <paramref name="pattern"/> parameter.
        /// </summary>
        /// <param name="encrypted">
        /// The encrypted value to decrypt.
        /// </param>
        /// <param name="pattern"></param>
        /// The pattern to use for decryption.
        /// <returns>
        /// The decrypted representation of the <paramref name="encrypted"/> parameter.
        /// </returns>
        public static string VigenereCipherDecrypt(string encrypted, params int[] pattern)
        {
            if (encrypted == null)
            {
                return null;
            }
            if (pattern?.Length == 0)
            {
                return encrypted;
            }

            StringBuilder builder = new StringBuilder();
            int patternIndex = 0;
            for (int i = 0; i < encrypted.Length; i++)
            {
                char pos = (char)(encrypted[i] - pattern[patternIndex]);
                builder.Append(char.MinValue > pos ? (char)(char.MaxValue + pos) : pos);
                patternIndex = (patternIndex + 1) >= pattern.Length ? 0 : patternIndex + 1;
            }
            return builder.ToString();
        }
    }
}
