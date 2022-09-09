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

    /// <summary>
    /// Event raised on the current context when the authentication is refreshed.
    /// </summary>
    public event EventHandler? AuthRefreshed;

    #endregion

    #region Initializers

    internal FirebaseUser(FirebaseAuth auth)
    {
        ArgumentNullException.ThrowIfNull(auth.IdToken);
        ArgumentNullException.ThrowIfNull(auth.RefreshToken);
        ArgumentNullException.ThrowIfNull(auth.ExpiresIn);
        ArgumentNullException.ThrowIfNull(auth.LocalId);

        IdToken = auth.IdToken;
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
