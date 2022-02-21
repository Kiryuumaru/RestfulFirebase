namespace RestfulFirebase.Local;

/// <summary>
/// The encryption declarations used for local database security.
/// </summary>
public interface ILocalEncryption
{
    /// <summary>
    /// Encrypt value from all incoming values to be save in local database.
    /// </summary>
    /// <param name="value">
    /// The value to encrypt.
    /// </param>
    /// <returns>
    /// The encrypted value to save in local database.
    /// </returns>
    string? Encrypt(string? value);

    /// <summary>
    /// Decrypt value from all incoming values saved from the local database.
    /// </summary>
    /// <param name="value">
    /// The encrypted value to decrypt.
    /// </param>
    /// <returns>
    /// The decrypted value from local database.
    /// </returns>
    string? Decrypt(string? value);
}
