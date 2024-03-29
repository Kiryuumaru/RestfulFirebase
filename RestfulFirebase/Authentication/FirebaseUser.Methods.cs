﻿using RestfulFirebase.Common.Utilities;
using System;
using RestfulFirebase.Authentication.Internals;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using System.Text.Json;
using RestfulFirebase.Authentication.Enums;
using System.Diagnostics.CodeAnalysis;
using RestfulHelpers.Common;
using static RestfulFirebase.Authentication.AuthenticationApi;

namespace RestfulFirebase.Authentication;

/// <summary>
/// Provides firebase user authentication implementations.
/// </summary>
public partial class FirebaseUser
{
    /// <summary>
    /// Decrypt the <see cref="FirebaseUser"/> using a series of interwoven Caesar ciphers <paramref name="data"/>.
    /// </summary>
    /// <param name="app">
    /// The <see cref="FirebaseApp"/> to use.
    /// </param>
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
    public static FirebaseUser Decrypt(FirebaseApp app, string data, params int[] pattern)
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

        return new(app, auth, created);
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

        auth = BlobSerializer.SetValue(auth, "tok", idToken);
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

    /// <summary>
    /// Check if the token is expired.
    /// </summary>
    /// <returns>
    /// <c>true</c> if the token is expired; otherwise, <c>false</c>.
    /// </returns>
    public bool IsExpired()
    {
        return DateTimeOffset.Now > Created.AddSeconds(ExpiresIn - 60);
    }

    /// <inheritdoc/>
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(FirebaseAuth))]
    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    public async ValueTask<HttpResponse<string>> GetFreshToken(CancellationToken cancellationToken = default)
    {
        HttpResponse<string> response = new();

        var refreshUserInfoResponse = await RefreshUserInfo(cancellationToken);
        response.Append(refreshUserInfoResponse);
        if (refreshUserInfoResponse.IsError)
        {
            return response;
        }

        response.Append(idToken);

        return response;
    }

    /// <summary>
    /// Request to send an email verification to the user`s email.
    /// </summary>
    /// <param name="cancellationToken">
    /// The <see cref="CancellationToken"/> that propagates notification if the operations should be canceled.
    /// </param>
    /// <returns>
    /// The <see cref="Task"/> proxy that represents the <see cref="HttpResponse"/>.
    /// </returns>
    public async Task<HttpResponse> SendEmailVerification(CancellationToken cancellationToken = default)
    {
        HttpResponse response = new();

        var tokenResponse = await GetFreshToken(cancellationToken);
        response.Append(tokenResponse);
        if (tokenResponse.IsError)
        {
            return response;
        }

        using MemoryStream stream = new();
        Utf8JsonWriter writer = new(stream);

        writer.WriteStartObject();
        writer.WritePropertyName("requestType");
        writer.WriteStringValue("VERIFY_EMAIL");
        writer.WritePropertyName("idToken");
        writer.WriteStringValue(tokenResponse.Result);
        writer.WriteEndObject();

        await writer.FlushAsync(cancellationToken);

        response.Append(await App.Authentication.ExecutePost(stream, GoogleGetConfirmationCodeUrl, cancellationToken));

        return response;
    }

    /// <summary>
    /// Request to change the email of the authenticated user.
    /// </summary>
    /// <param name="newEmail">
    /// The new email.
    /// </param>
    /// <param name="cancellationToken">
    /// The <see cref="CancellationToken"/> that propagates notification if the operations should be canceled.
    /// </param>
    /// <returns>
    /// The <see cref="Task"/> proxy that represents the <see cref="HttpResponse"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="newEmail"/> is a null reference.
    /// </exception>
    public async Task<HttpResponse> ChangeUserEmail(string newEmail, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(newEmail);

        HttpResponse response = new();

        var tokenResponse = await GetFreshToken(cancellationToken);
        response.Append(tokenResponse);
        if (tokenResponse.IsError)
        {
            return response;
        }

        using MemoryStream stream = new();
        Utf8JsonWriter writer = new(stream);

        writer.WriteStartObject();
        writer.WritePropertyName("email");
        writer.WriteStringValue(newEmail);
        writer.WritePropertyName("returnSecureToken");
        writer.WriteBooleanValue(true);
        writer.WritePropertyName("idToken");
        writer.WriteStringValue(tokenResponse.Result);
        writer.WriteEndObject();

        await writer.FlushAsync(cancellationToken);

        response.Append(await ExecuteUser(stream, GoogleUpdateUser, cancellationToken));

        return response;
    }

    /// <summary>
    /// Request to change the password of the authenticated user.
    /// </summary>
    /// <param name="newPassword">
    /// The new password.
    /// </param>
    /// <param name="cancellationToken">
    /// The <see cref="CancellationToken"/> that propagates notification if the operations should be canceled.
    /// </param>
    /// <returns>
    /// The <see cref="Task"/> proxy that represents the <see cref="HttpResponse"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="newPassword"/> is a null reference.
    /// </exception>
    public async Task<HttpResponse> ChangeUserPassword(string newPassword, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(newPassword);

        HttpResponse response = new();

        var tokenResponse = await GetFreshToken(cancellationToken);
        response.Append(tokenResponse);
        if (tokenResponse.IsError)
        {
            return response;
        }

        using MemoryStream stream = new();
        Utf8JsonWriter writer = new(stream);

        writer.WriteStartObject();
        writer.WritePropertyName("password");
        writer.WriteStringValue(newPassword);
        writer.WritePropertyName("returnSecureToken");
        writer.WriteBooleanValue(true);
        writer.WritePropertyName("idToken");
        writer.WriteStringValue(tokenResponse.Result);
        writer.WriteEndObject();

        await writer.FlushAsync(cancellationToken);

        response.Append(await ExecuteUser(stream, GoogleUpdateUser, cancellationToken));

        return response;
    }

    /// <summary>
    /// Request to change the password of the authenticated user.
    /// </summary>
    /// <param name="displayName">
    /// The new display name of the account.
    /// </param>
    /// <param name="photoUrl">
    /// The new photo url of the account.
    /// </param>
    /// <param name="cancellationToken">
    /// The <see cref="CancellationToken"/> that propagates notification if the operations should be canceled.
    /// </param>
    /// <returns>
    /// The <see cref="Task"/> proxy that represents the <see cref="HttpResponse"/>.
    /// </returns>
    public async Task<HttpResponse> UpdateProfile(string? displayName, string? photoUrl, CancellationToken cancellationToken = default)
    {
        HttpResponse response = new();

        var tokenResponse = await GetFreshToken(cancellationToken);
        response.Append(tokenResponse);
        if (tokenResponse.IsError)
        {
            return response;
        }

        using MemoryStream stream = new();
        Utf8JsonWriter writer = new(stream);

        writer.WriteStartObject();
        if (!string.IsNullOrWhiteSpace(displayName) && !string.IsNullOrWhiteSpace(photoUrl))
        {
            writer.WritePropertyName("displayName");
            writer.WriteStringValue(displayName);
            writer.WritePropertyName("photoUrl");
            writer.WriteStringValue(photoUrl);
        }
        else if (!string.IsNullOrWhiteSpace(displayName))
        {
            writer.WritePropertyName("displayName");
            writer.WriteStringValue(displayName);
            writer.WritePropertyName("deleteAttribute");
            writer.WriteStartArray();
            writer.WriteStringValue("PHOTO_URL");
            writer.WriteEndArray();
        }
        else if (!string.IsNullOrWhiteSpace(photoUrl))
        {
            writer.WritePropertyName("photoUrl");
            writer.WriteStringValue(photoUrl);
            writer.WritePropertyName("deleteAttribute");
            writer.WriteStartArray();
            writer.WriteStringValue("DISPLAY_NAME");
            writer.WriteEndArray();
        }
        else
        {
            writer.WritePropertyName("deleteAttribute");
            writer.WriteStartArray();
            writer.WriteStringValue("DISPLAY_NAME");
            writer.WriteStringValue("PHOTO_URL");
            writer.WriteEndArray();
        }
        writer.WritePropertyName("returnSecureToken");
        writer.WriteBooleanValue(true);
        writer.WritePropertyName("idToken");
        writer.WriteStringValue(tokenResponse.Result);
        writer.WriteEndObject();

        await writer.FlushAsync(cancellationToken);

        response.Append(await ExecuteUser(stream, GoogleSetAccountUrl, cancellationToken));

        return response;
    }

    /// <summary>
    /// Request to delete the authenticated user.
    /// </summary>
    /// <param name="cancellationToken">
    /// The <see cref="CancellationToken"/> that propagates notification if the operations should be canceled.
    /// </param>
    /// <returns>
    /// The <see cref="Task"/> proxy that represents the <see cref="HttpResponse"/>.
    /// </returns>
    public async Task<HttpResponse> DeleteUser(CancellationToken cancellationToken = default)
    {
        HttpResponse response = new();

        var tokenResponse = await GetFreshToken(cancellationToken);
        response.Append(tokenResponse);
        if (tokenResponse.IsError)
        {
            return response;
        }

        using MemoryStream stream = new();
        Utf8JsonWriter writer = new(stream);

        writer.WriteStartObject();
        writer.WritePropertyName("idToken");
        writer.WriteStringValue(tokenResponse.Result);
        writer.WriteEndObject();

        await writer.FlushAsync(cancellationToken);

        response.Append(await App.Authentication.ExecutePost(stream, GoogleDeleteUserUrl, cancellationToken));

        return response;
    }

    /// <summary>
    /// Request to delete the authenticated user.
    /// </summary>
    /// <param name="email">
    /// The account`s email to be linked.
    /// </param>
    /// <param name="password">
    /// The account`s password to be linked.
    /// </param>
    /// <param name="cancellationToken">
    /// The <see cref="CancellationToken"/> that propagates notification if the operations should be canceled.
    /// </param>
    /// <returns>
    /// The <see cref="Task"/> proxy that represents the <see cref="HttpResponse"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="email"/> or
    /// <paramref name="password"/> is a null reference.
    /// </exception>
    public async Task<HttpResponse> LinkAccount(string email, string password, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(email);
        ArgumentNullException.ThrowIfNull(password);

        HttpResponse response = new();

        var tokenResponse = await GetFreshToken(cancellationToken);
        response.Append(tokenResponse);
        if (tokenResponse.IsError)
        {
            return response;
        }

        using MemoryStream stream = new();
        Utf8JsonWriter writer = new(stream);

        writer.WriteStartObject();
        writer.WritePropertyName("email");
        writer.WriteStringValue(email);
        writer.WritePropertyName("password");
        writer.WriteStringValue(password);
        writer.WritePropertyName("returnSecureToken");
        writer.WriteBooleanValue(true);
        writer.WritePropertyName("idToken");
        writer.WriteStringValue(tokenResponse.Result);
        writer.WriteEndObject();

        await writer.FlushAsync(cancellationToken);

        response.Append(await ExecuteUser(stream, GoogleSetAccountUrl, cancellationToken));

        return response;
    }

    /// <summary>
    /// Request to link the account with oauth provided with auth type and oauth access token.
    /// </summary>
    /// <param name="authType">
    /// The <see cref="FirebaseAuthType"/> to be linked.
    /// </param>
    /// <param name="oauthAccessToken">
    /// The token of the provided auth type to be linked.
    /// </param>
    /// <param name="cancellationToken">
    /// The <see cref="CancellationToken"/> that propagates notification if the operations should be canceled.
    /// </param>
    /// <returns>
    /// The <see cref="Task"/> proxy that represents the <see cref="HttpResponse"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="oauthAccessToken"/> is a null reference.
    /// </exception>
    public async Task<HttpResponse> LinkAccount(FirebaseAuthType authType, string oauthAccessToken, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(oauthAccessToken);

        HttpResponse response = new();

        var tokenResponse = await GetFreshToken(cancellationToken);
        response.Append(tokenResponse);
        if (tokenResponse.IsError)
        {
            return response;
        }

        var providerId = GetProviderId(authType);

        using MemoryStream stream = new();
        Utf8JsonWriter writer = new(stream);

        writer.WriteStartObject();
        writer.WritePropertyName("postBody");
        writer.WriteStringValue($"access_token={oauthAccessToken}&providerId={providerId}");
        writer.WritePropertyName("requestUri");
        writer.WriteStringValue("http://localhost");
        writer.WritePropertyName("returnSecureToken");
        writer.WriteBooleanValue(true);
        writer.WritePropertyName("idToken");
        writer.WriteStringValue(tokenResponse.Result);
        writer.WriteEndObject();

        await writer.FlushAsync(cancellationToken);

        response.Append(await ExecuteUser(stream, GoogleIdentityUrl, cancellationToken));

        return response;
    }

    /// <summary>
    /// Request to unlink the account with oauth provided with auth type.
    /// </summary>
    /// <param name="authType">
    /// The <see cref="FirebaseAuthType"/> to unlinked.
    /// </param>
    /// <param name="cancellationToken">
    /// The <see cref="CancellationToken"/> that propagates notification if the operations should be canceled.
    /// </param>
    /// <returns>
    /// The <see cref="Task"/> proxy that represents the <see cref="HttpResponse"/>.
    /// </returns>
    public async Task<HttpResponse> UnlinkAccounts(FirebaseAuthType authType, CancellationToken cancellationToken = default)
    {
        HttpResponse response = new();

        var tokenResponse = await GetFreshToken(cancellationToken);
        response.Append(tokenResponse);
        if (tokenResponse.IsError)
        {
            return response;
        }

        string? providerId;
        if (authType == FirebaseAuthType.EmailAndPassword)
        {
            providerId = authType.ToEnumString();
        }
        else
        {
            providerId = GetProviderId(authType);
        }

        using MemoryStream stream = new();
        Utf8JsonWriter writer = new(stream);

        writer.WriteStartObject();
        writer.WritePropertyName("deleteProvider");
        writer.WriteStringValue(providerId);
        writer.WritePropertyName("idToken");
        writer.WriteStringValue(tokenResponse.Result);
        writer.WriteEndObject();

        await writer.FlushAsync(cancellationToken);

        response.Append(await ExecuteUser(stream, GoogleSetAccountUrl, cancellationToken));

        return response;
    }
}
