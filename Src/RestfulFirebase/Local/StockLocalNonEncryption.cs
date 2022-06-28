namespace RestfulFirebase.Local;

/// <summary>
/// The provided stock <see cref="ILocalEncryption"/> implementation to be used.
/// </summary>
public sealed class StockLocalNonEncryption : ILocalEncryption
{
    /// <summary>
    /// Creates new instance of <see cref="StockLocalNonEncryption"/> class.
    /// </summary>
    public StockLocalNonEncryption()
    {

    }

    /// <inheritdoc/>
    public string? Decrypt(string? value)
    {
        return value;
    }

    /// <inheritdoc/>
    public string? Encrypt(string? value)
    {
        return value;
    }
}
