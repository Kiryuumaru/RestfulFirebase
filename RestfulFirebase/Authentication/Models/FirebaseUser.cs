using RestfulFirebase.Common.Utilities;
using System;
using RestfulFirebase.Authentication.Internals;
using RestfulFirebase.Common.Abstractions;

namespace RestfulFirebase.Authentication.Models;

/// <summary>
/// Provides firebase user authentication implementations.
/// </summary>
public class FirebaseUser : IAuthorization
{
    #region Properties

    /// <summary>
    /// Gets the firebase token of the authenticated account which can be used for authenticated queries. 
    /// </summary>
    public string IdToken { get; private set; }

    /// <summary>
    /// Gets the refresh token of the underlying service which can be used to get a new access token. 
    /// </summary>
    public string RefreshToken { get; private set; }

    /// <summary>
    /// Gets the number of seconds since the token is created.
    /// </summary>
    public int ExpiresIn { get; private set; }

    /// <summary>
    /// Gets the <see cref="DateTimeOffset"/> when this token was created.
    /// </summary>
    public DateTimeOffset Created { get; private set; }

    /// <summary>
    /// Gets the local id or the <c>UID</c> of the account.
    /// </summary>
    public string LocalId { get; private set; }

    /// <summary>
    /// Gets the federated id of the account.
    /// </summary>
    public string? FederatedId { get; private set; }

    /// <summary>
    /// Gets the first name of the user.
    /// </summary>
    public string? FirstName { get; private set; }

    /// <summary>
    /// Gets the last name of the user.
    /// </summary>
    public string? LastName { get; private set; }

    /// <summary>
    /// Gets the display name of the user.
    /// </summary>
    public string? DisplayName { get; private set; }

    /// <summary>
    /// Gets the email of the user.
    /// </summary>
    public string? Email { get; private set; }

    /// <summary>
    /// Gets the email verfication status of the account.
    /// </summary>
    public bool IsEmailVerified { get; private set; }

    /// <summary>
    /// Gets or sets the photo url of the account.
    /// </summary>
    public string? PhotoUrl { get; private set; }

    /// <summary>
    /// Gets or sets the phone number of the user.
    /// </summary>
    public string? PhoneNumber { get; private set; }

    /// <inheritdoc/>
    public string Token => IdToken;

    /// <inheritdoc/>
    public bool IsAccessToken => false;

    /// <summary>
    /// Event raised on the current context when the authentication is refreshed.
    /// </summary>
    public event EventHandler? AuthRefreshed;

    #endregion

    #region Initializers

    internal FirebaseUser(FirebaseAuth auth)
        : this(auth, DateTimeOffset.UtcNow)
    {

    }

    internal FirebaseUser(FirebaseAuth auth, DateTimeOffset created)
    {
        ArgumentNullException.ThrowIfNull(auth.IdToken);
        ArgumentNullException.ThrowIfNull(auth.RefreshToken);
        ArgumentNullException.ThrowIfNull(auth.ExpiresIn);
        ArgumentNullException.ThrowIfNull(auth.LocalId);

        IdToken = auth.IdToken;
        RefreshToken = auth.RefreshToken;
        ExpiresIn = auth.ExpiresIn.Value;
        LocalId = auth.LocalId;

        Created = created;

        UpdateAuth(auth);
    }

    /// <summary>
    /// Decrypt the <see cref="FirebaseUser"/> using a series of interwoven Caesar ciphers <paramref name="data"/>.
    /// </summary>
    /// <param name="pattern">
    /// The pattern to use for decryption.
    /// </param>
    /// <param name="data">
    /// The encrypted data.
    /// </param>
    /// <returns>
    /// The decrypted <see cref="FirebaseAuth"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="data"/> or
    /// <paramref name="pattern"/> is a null reference.
    /// </exception>
    public static FirebaseUser Decrypt(string data, params int[] pattern)
    {
        string decrypted = Cryptography.VigenereCipherDecrypt(data, pattern);

        string? idToken = BlobSerializer.GetValue(decrypted, "tok");
        string? refreshToken = BlobSerializer.GetValue(decrypted, "ref");
        var exp = BlobSerializer.GetValue(decrypted, "exp");
        int expiresIn = string.IsNullOrEmpty(exp) ? default : (int)StringSerializer.ExtractNumber(exp!);
        var ctd = BlobSerializer.GetValue(decrypted, "ctd");
        DateTimeOffset created = string.IsNullOrEmpty(ctd) ? default : new DateTimeOffset(StringSerializer.ExtractNumber(ctd!), DateTimeOffset.Now.Offset);
        string? localId = BlobSerializer.GetValue(decrypted, "lid");
        string? federatedId = BlobSerializer.GetValue(decrypted, "fid");
        string? firstName = BlobSerializer.GetValue(decrypted, "fname");
        string? lastName = BlobSerializer.GetValue(decrypted, "lname");
        string? displayName = BlobSerializer.GetValue(decrypted, "dname");
        string? email = BlobSerializer.GetValue(decrypted, "email");
        bool isEmailVerified = BlobSerializer.GetValue(decrypted, "vmail") == "1";
        string? photoUrl = BlobSerializer.GetValue(decrypted, "purl");
        string? phoneNumber = BlobSerializer.GetValue(decrypted, "pnum");

        FirebaseAuth auth = new()
        {
            IdToken = idToken,
            RefreshToken = refreshToken,
            ExpiresIn = expiresIn,
            LocalId = localId,
            FederatedId = federatedId,
            FirstName = firstName,
            LastName = lastName,
            DisplayName = displayName,
            Email = email,
            IsEmailVerified = isEmailVerified,
            PhotoUrl = photoUrl,
            PhoneNumber = phoneNumber
        };

        return new(auth, created);
    }

    #endregion

    #region Methods

    /// <summary>
    /// Check if the token is expired.
    /// </summary>
    /// <returns>
    /// <c>true</c> if the token is expired; otherwise, <c>false</c>.
    /// </returns>
    public bool IsExpired()
    {
        return DateTimeOffset.Now > Created.AddSeconds(ExpiresIn - 10);
    }

    /// <summary>
    /// Invokes if the auth has changes.
    /// </summary>
    protected void OnAuthRefreshed()
    {
        AuthRefreshed?.Invoke(this, new EventArgs());
    }

    /// <summary>
    /// Encrypt the <see cref="FirebaseUser"/> to <see cref="string"/> using a series of interwoven Caesar ciphers.
    /// </summary>
    /// <param name="pattern">
    /// The pattern to use for encryption.
    /// </param>
    /// <returns>
    /// The encrypted data.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="pattern"/> is a null reference.
    /// </exception>
    public string Encrypt(params int[] pattern)
    {
        var auth = "";

        auth = BlobSerializer.SetValue(auth, "tok", IdToken);
        auth = BlobSerializer.SetValue(auth, "ref", RefreshToken);
        auth = BlobSerializer.SetValue(auth, "exp", StringSerializer.CompressNumber(ExpiresIn));
        auth = BlobSerializer.SetValue(auth, "ctd", StringSerializer.CompressNumber(Created.Ticks));
        auth = BlobSerializer.SetValue(auth, "lid", LocalId);
        auth = BlobSerializer.SetValue(auth, "fid", FederatedId);
        auth = BlobSerializer.SetValue(auth, "fname", FirstName);
        auth = BlobSerializer.SetValue(auth, "lname", LastName);
        auth = BlobSerializer.SetValue(auth, "dname", DisplayName);
        auth = BlobSerializer.SetValue(auth, "email", Email);
        auth = BlobSerializer.SetValue(auth, "vmail", IsEmailVerified ? "1" : "0");
        auth = BlobSerializer.SetValue(auth, "purl", PhotoUrl);
        auth = BlobSerializer.SetValue(auth, "pnum", PhoneNumber);

        return Cryptography.VigenereCipherEncrypt(auth, pattern);
    }

    internal void UpdateAuth(FirebaseAuth auth)
    {
        bool hasChanges = false;

        if (auth.IdToken != null && auth.IdToken != IdToken)
        {
            IdToken = auth.IdToken;
            Created = DateTime.UtcNow;
            hasChanges = true;
        }
        if (auth.RefreshToken != null && auth.RefreshToken != RefreshToken)
        {
            RefreshToken = auth.RefreshToken;
            hasChanges = true;
        }
        if (auth.ExpiresIn.HasValue && auth.ExpiresIn.Value != ExpiresIn)
        {
            ExpiresIn = auth.ExpiresIn.Value;
            hasChanges = true;
        }
        if (auth.LocalId != null && auth.LocalId != LocalId)
        {
            LocalId = auth.LocalId;
            hasChanges = true;
        }

        if (hasChanges)
        {
            OnAuthRefreshed();
        }
    }

    internal void UpdateInfo(FirebaseAuth auth)
    {
        bool hasChanges = false;

        if (FederatedId != auth.FederatedId)
        {
            FederatedId = auth.FederatedId;
            hasChanges = true;
        }
        if (FirstName != auth.FirstName)
        {
            FirstName = auth.FirstName;
            hasChanges = true;
        }
        if (LastName != auth.LastName)
        {
            LastName = auth.LastName;
            hasChanges = true;
        }
        if (DisplayName != auth.DisplayName)
        {
            DisplayName = auth.DisplayName;
            hasChanges = true;
        }
        if (Email != auth.Email)
        {
            Email = auth.Email;
            hasChanges = true;
        }
        if (IsEmailVerified != auth.IsEmailVerified)
        {
            IsEmailVerified = auth.IsEmailVerified;
            hasChanges = true;
        }
        if (PhoneNumber != auth.PhoneNumber)
        {
            PhoneNumber = auth.PhoneNumber;
            hasChanges = true;
        }

        if (hasChanges)
        {
            OnAuthRefreshed();
        }
    }

    #endregion
}
