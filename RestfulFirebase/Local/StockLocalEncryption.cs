using RestfulFirebase.Utilities;

namespace RestfulFirebase.Local;

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
    public string? Decrypt(string? value)
    {
        return Cryptography.VigenereCipherDecrypt(value, EncryptionPattern);
    }

    /// <inheritdoc/>
    public string? Encrypt(string? value)
    {
        return Cryptography.VigenereCipherEncrypt(value, EncryptionPattern);
    }
}
