using RestfulFirebase.Common.Utilities;
using System;
using RestfulFirebase.Authentication.Internals;
using RestfulFirebase.Common.Abstractions;
using ObservableHelpers.ComponentModel;
using ObservableHelpers.ComponentModel.Enums;

namespace RestfulFirebase.Authentication.Models;

/// <summary>
/// Provides firebase user authentication implementations.
/// </summary>
[ObservableObject]
public partial class FirebaseUser : IAuthorization
{
    #region Properties

    /// <summary>
    /// Gets the firebase token of the authenticated account which can be used for authenticated queries. 
    /// </summary>
    [ObservableProperty(Access = AccessModifier.PublicWithPrivateSetter)]
    [NotifyPropertyChangedFor(nameof(Token))]
    string idToken;

    /// <summary>
    /// Gets the refresh token of the underlying service which can be used to get a new access token. 
    /// </summary>
    [ObservableProperty(Access = AccessModifier.PublicWithPrivateSetter)]
    string refreshToken;

    /// <summary>
    /// Gets the number of seconds since the token is created.
    /// </summary>
    [ObservableProperty(Access = AccessModifier.PublicWithPrivateSetter)]
    int expiresIn;

    /// <summary>
    /// Gets the <see cref="DateTimeOffset"/> when this token was created.
    /// </summary>
    [ObservableProperty(Access = AccessModifier.PublicWithPrivateSetter)]
    DateTimeOffset created;

    /// <summary>
    /// Gets the local id or the <c>UID</c> of the account.
    /// </summary>
    [ObservableProperty(Access = AccessModifier.PublicWithPrivateSetter)]
    string localId;

    /// <summary>
    /// Gets the federated id of the account.
    /// </summary>
    [ObservableProperty(Access = AccessModifier.PublicWithPrivateSetter)]
    string? federatedId;

    /// <summary>
    /// Gets the first name of the user.
    /// </summary>
    [ObservableProperty(Access = AccessModifier.PublicWithPrivateSetter)]
    string? firstName;

    /// <summary>
    /// Gets the last name of the user.
    /// </summary>
    [ObservableProperty(Access = AccessModifier.PublicWithPrivateSetter)]
    string? lastName;

    /// <summary>
    /// Gets the display name of the user.
    /// </summary>
    [ObservableProperty(Access = AccessModifier.PublicWithPrivateSetter)]
    string? displayName;

    /// <summary>
    /// Gets the email of the user.
    /// </summary>
    [ObservableProperty(Access = AccessModifier.PublicWithPrivateSetter)]
    string? email;

    /// <summary>
    /// Gets the email verfication status of the account.
    /// </summary>
    [ObservableProperty(Access = AccessModifier.PublicWithPrivateSetter)]
    bool isEmailVerified;

    /// <summary>
    /// Gets or sets the photo url of the account.
    /// </summary>
    [ObservableProperty(Access = AccessModifier.PublicWithPrivateSetter)]
    string? photoUrl;

    /// <summary>
    /// Gets or sets the phone number of the user.
    /// </summary>
    [ObservableProperty(Access = AccessModifier.PublicWithPrivateSetter)]
    string? phoneNumber;

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
        : this(auth, DateTimeOffset.Now)
    {

    }

    internal FirebaseUser(FirebaseAuth auth, DateTimeOffset created)
    {
        ArgumentNullException.ThrowIfNull(auth.IdToken);
        ArgumentNullException.ThrowIfNull(auth.RefreshToken);
        ArgumentNullException.ThrowIfNull(auth.ExpiresIn);
        ArgumentNullException.ThrowIfNull(auth.LocalId);

        idToken = auth.IdToken;
        refreshToken = auth.RefreshToken;
        expiresIn = auth.ExpiresIn.Value;
        localId = auth.LocalId;

        this.created = created;

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
        DateTimeOffset created = string.IsNullOrEmpty(ctd) ? default : new DateTimeOffset(StringSerializer.ExtractNumber(ctd!), DateTimeOffset.UtcNow.Offset);
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
        auth = BlobSerializer.SetValue(auth, "ctd", StringSerializer.CompressNumber(Created.ToUniversalTime().Ticks));
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
        if (auth.IdToken != null && auth.IdToken != IdToken)
        {
            IdToken = auth.IdToken;
            Created = DateTimeOffset.UtcNow;
        }
        if (auth.RefreshToken != null && auth.RefreshToken != RefreshToken)
        {
            RefreshToken = auth.RefreshToken;
        }
        if (auth.ExpiresIn.HasValue && auth.ExpiresIn.Value != ExpiresIn)
        {
            ExpiresIn = auth.ExpiresIn.Value;
        }
        if (auth.LocalId != null && auth.LocalId != LocalId)
        {
            LocalId = auth.LocalId;
        }

        UpdateInfo(auth);
    }

    internal void UpdateInfo(FirebaseAuth auth)
    {
        FederatedId = auth.FederatedId;
        FirstName = auth.FirstName;
        LastName = auth.LastName;
        DisplayName = auth.DisplayName;
        Email = auth.Email;
        IsEmailVerified = auth.IsEmailVerified;
        PhoneNumber = auth.PhoneNumber;
    }

    #endregion
}
