using RestfulFirebase.Exceptions;
using RestfulFirebase.Utilities;
using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json;
using DisposableHelpers;
using DisposableHelpers.Attributes;
using RestfulFirebase.Authentication.Internals;

namespace RestfulFirebase.Authentication;

/// <summary>
/// Provides firebase user authentication implementations.
/// </summary>
public class FirebaseUser
{
    #region Properties

    /// <summary>
    /// Gets the firebase token of the authenticated account which can be used for authenticated queries. 
    /// </summary>
    public string FirebaseToken { get; private set; }

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

    /// <summary>
    /// Event raised on the current context when the authentication is refreshed.
    /// </summary>
    public event EventHandler? AuthRefreshed;

    #endregion

    #region Initializers

    internal FirebaseUser(FirebaseAuth auth)
    {
        ArgumentNullException.ThrowIfNull(auth.FirebaseToken);
        ArgumentNullException.ThrowIfNull(auth.RefreshToken);
        ArgumentNullException.ThrowIfNull(auth.ExpiresIn);
        ArgumentNullException.ThrowIfNull(auth.LocalId);

        FirebaseToken = auth.FirebaseToken;
        RefreshToken = auth.RefreshToken;
        ExpiresIn = auth.ExpiresIn.Value;
        LocalId = auth.LocalId;

        Created = DateTime.UtcNow;

        UpdateAuth(auth);
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

    internal void UpdateAuth(FirebaseAuth auth)
    {
        if (auth.FirebaseToken != null && !string.IsNullOrEmpty(auth.FirebaseToken))
        {
            if (FirebaseToken != auth.FirebaseToken)
            {
                Created = DateTime.UtcNow;
            }
            FirebaseToken = auth.FirebaseToken;
        }
        if (auth.RefreshToken != null && !string.IsNullOrEmpty(auth.RefreshToken))
        {
            RefreshToken = auth.RefreshToken;
        }
        if (auth.ExpiresIn.HasValue)
        {
            ExpiresIn = auth.ExpiresIn.Value;
        }
        if (auth.LocalId != null && !string.IsNullOrEmpty(auth.FirebaseToken))
        {
            LocalId = auth.LocalId;
        }

        FederatedId = auth.FederatedId;
        FirstName = auth.FirstName;
        LastName = auth.LastName;
        DisplayName = auth.DisplayName;
        Email = auth.Email;
        IsEmailVerified = auth.IsEmailVerified;
        PhotoUrl = auth.PhoneNumber;
        PhoneNumber = auth.PhoneNumber;

        OnAuthRefreshed();
    }

    #endregion
}
