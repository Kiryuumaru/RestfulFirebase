using System;
using RestfulFirebase.Authentication.Internals;
using RestfulFirebase.Common.Abstractions;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections.Generic;

namespace RestfulFirebase.Authentication;

/// <summary>
/// Provides firebase user authentication implementations.
/// </summary>
public partial class FirebaseUser : IAuthorization, INotifyPropertyChanged, INotifyPropertyChanging
{
    /// <summary>
    /// Gets the refresh token of the underlying service which can be used to get a new access token. 
    /// </summary>
    public string RefreshToken
    {
        get => refreshToken;
        private set
        {
            if (!EqualityComparer<string>.Default.Equals(refreshToken, value))
            {
                OnPropertyChanging();
                refreshToken = value;
                OnPropertyChanged();
            }
        }
    }
    string refreshToken;

    /// <summary>
    /// Gets the number of seconds since the token is created.
    /// </summary>
    public int ExpiresIn
    {
        get => expiresIn;
        private set
        {
            if (!EqualityComparer<int>.Default.Equals(expiresIn, value))
            {
                OnPropertyChanging();
                expiresIn = value;
                OnPropertyChanged();
            }
        }
    }
    int expiresIn;

    /// <summary>
    /// Gets the <see cref="DateTimeOffset"/> when this token was created.
    /// </summary>
    public DateTimeOffset Created
    {
        get => created;
        private set
        {
            if (!EqualityComparer<DateTimeOffset>.Default.Equals(created, value))
            {
                OnPropertyChanging();
                created = value;
                OnPropertyChanged();
            }
        }
    }
    DateTimeOffset created;

    /// <summary>
    /// Gets the local id or the <c>UID</c> of the account.
    /// </summary>
    public string LocalId
    {
        get => localId;
        private set
        {
            if (!EqualityComparer<string>.Default.Equals(localId, value))
            {
                OnPropertyChanging();
                localId = value;
                OnPropertyChanged();
            }
        }
    }
    string localId;

    /// <summary>
    /// Gets the federated id of the account.
    /// </summary>
    public string? FederatedId
    {
        get => federatedId;
        private set
        {
            if (!EqualityComparer<string?>.Default.Equals(federatedId, value))
            {
                OnPropertyChanging();
                federatedId = value;
                OnPropertyChanged();
            }
        }
    }
    string? federatedId;

    /// <summary>
    /// Gets the first name of the user.
    /// </summary>
    public string? FirstName
    {
        get => firstName;
        private set
        {
            if (!EqualityComparer<string?>.Default.Equals(firstName, value))
            {
                OnPropertyChanging();
                firstName = value;
                OnPropertyChanged();
            }
        }
    }
    string? firstName;

    /// <summary>
    /// Gets the last name of the user.
    /// </summary>
    public string? LastName
    {
        get => lastName;
        private set
        {
            if (!EqualityComparer<string?>.Default.Equals(lastName, value))
            {
                OnPropertyChanging();
                lastName = value;
                OnPropertyChanged();
            }
        }
    }
    string? lastName;

    /// <summary>
    /// Gets the display name of the user.
    /// </summary>
    public string? DisplayName
    {
        get => displayName;
        private set
        {
            if (!EqualityComparer<string?>.Default.Equals(displayName, value))
            {
                OnPropertyChanging();
                displayName = value;
                OnPropertyChanged();
            }
        }
    }
    string? displayName;

    /// <summary>
    /// Gets the email of the user.
    /// </summary>
    public string? Email
    {
        get => email;
        private set
        {
            if (!EqualityComparer<string?>.Default.Equals(email, value))
            {
                OnPropertyChanging();
                email = value;
                OnPropertyChanged();
            }
        }
    }
    string? email;

    /// <summary>
    /// Gets the email verfication status of the account.
    /// </summary>
    public bool IsEmailVerified
    {
        get => isEmailVerified;
        private set
        {
            if (!EqualityComparer<bool>.Default.Equals(isEmailVerified, value))
            {
                OnPropertyChanging();
                isEmailVerified = value;
                OnPropertyChanged();
            }
        }
    }
    bool isEmailVerified;

    /// <summary>
    /// Gets or sets the photo url of the account.
    /// </summary>
    public string? PhotoUrl
    {
        get => photoUrl;
        private set
        {
            if (!EqualityComparer<string?>.Default.Equals(photoUrl, value))
            {
                OnPropertyChanging();
                photoUrl = value;
                OnPropertyChanged();
            }
        }
    }
    string? photoUrl;

    /// <summary>
    /// Gets or sets the phone number of the user.
    /// </summary>
    public string? PhoneNumber
    {
        get => phoneNumber;
        private set
        {
            if (!EqualityComparer<string?>.Default.Equals(phoneNumber, value))
            {
                OnPropertyChanging();
                phoneNumber = value;
                OnPropertyChanged();
            }
        }
    }
    string? phoneNumber;

    /// <inheritdoc/>
    public bool IsAccessToken => false;

    /// <summary>
    /// Gets the <see cref="FirebaseApp"/> used.
    /// </summary>
    public FirebaseApp App { get; }

    /// <inheritdoc/>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <inheritdoc/>
    public event PropertyChangingEventHandler? PropertyChanging;

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

    /// <summary>
    /// Raises the <see cref = "PropertyChanged"/> event.
    /// </summary>
    /// <param name = "propertyName">
    /// (optional) The name of the property that changed.
    /// </param>
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
    }

    /// <summary>
    /// Raises the <see cref = "PropertyChanging"/> event.
    /// </summary>
    /// <param name = "propertyName">
    /// (optional) The name of the property that changed.
    /// </param>
    protected void OnPropertyChanging([CallerMemberName] string? propertyName = null)
    {
        OnPropertyChanging(new PropertyChangingEventArgs(propertyName));
    }

    /// <summary>
    /// Raises <see cref = "PropertyChanged"/>.
    /// </summary>
    /// <param name = "e">
    /// The input <see cref = "PropertyChangedEventArgs"/> instance.
    /// </param>
    protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        PropertyChanged?.Invoke(this, e);
    }

    /// <summary>
    /// Raises <see cref = "PropertyChanging"/>.
    /// </summary>
    /// <param name = "e">
    /// The input <see cref = "PropertyChangingEventArgs"/> instance.
    /// </param>
    protected virtual void OnPropertyChanging(PropertyChangingEventArgs e)
    {
        PropertyChanging?.Invoke(this, e);
    }
}
