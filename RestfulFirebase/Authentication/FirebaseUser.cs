using System;
using RestfulFirebase.Authentication.Internals;
using RestfulFirebase.Common.Abstractions;
using ObservableHelpers.ComponentModel;
using ObservableHelpers.ComponentModel.Enums;

namespace RestfulFirebase.Authentication;

/// <summary>
/// Provides firebase user authentication implementations.
/// </summary>
[ObservableObject]
public partial class FirebaseUser : IAuthorization
{
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
    public bool IsAccessToken => false;

    /// <summary>
    /// Gets the <see cref="FirebaseApp"/> used.
    /// </summary>
    public FirebaseApp App { get; }

    private string idToken;

    internal FirebaseUser(FirebaseApp app, FirebaseAuth auth)
        : this(app, auth, DateTimeOffset.Now)
    {

    }

    internal FirebaseUser(FirebaseApp app, FirebaseAuth auth, DateTimeOffset created)
    {
        ArgumentNullException.ThrowIfNull(auth.IdToken);
        ArgumentNullException.ThrowIfNull(auth.RefreshToken);
        ArgumentNullException.ThrowIfNull(auth.ExpiresIn);
        ArgumentNullException.ThrowIfNull(auth.LocalId);

        App = app;

        idToken = auth.IdToken;
        refreshToken = auth.RefreshToken;
        expiresIn = auth.ExpiresIn.Value;
        localId = auth.LocalId;

        this.created = created;

        UpdateAuth(auth);
        UpdateInfo(auth);
    }
}
