using RestfulFirebase.Common.Utilities;
using System;
using RestfulFirebase.Authentication.Internals;
using RestfulFirebase.Common.Abstractions;
using ObservableHelpers.ComponentModel;
using ObservableHelpers.ComponentModel.Enums;
using System.Threading.Tasks;
using System.Threading;
using System.Net;
using System.IO;
using System.Text.Json;
using RestfulFirebase.Common.Http;
using System.Net.Http;
using RestfulFirebase.Common.Models;
using System.Text;
using RestfulFirebase.Authentication.Enums;
using System.Diagnostics.CodeAnalysis;

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
#if NET5_0_OR_GREATER
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(FirebaseAuth))]
    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
#endif
    public async ValueTask<Response<string>> GetFreshToken(CancellationToken cancellationToken = default)
    {
        if (!IsExpired())
        {
            return new Response<string>(idToken, null);
        }

        using MemoryStream stream = new();
        Utf8JsonWriter writer = new(stream);

        writer.WriteStartObject();
        writer.WritePropertyName("grant_type");
        writer.WriteStringValue("refresh_token");
        writer.WritePropertyName("refresh_token");
        writer.WriteStringValue(RefreshToken);
        writer.WriteEndObject();

        await writer.FlushAsync(cancellationToken);

        var response = await HttpHelpers.ExecuteWithContent<FirebaseAuth>(App.GetClient(), stream, HttpMethod.Post, AuthenticationApi.GoogleRefreshAuth, JsonSerializerHelpers.SnakeCaseJsonSerializerOption, cancellationToken);
        if (response.IsError)
        {
            return new HttpResponse<string>(null, response);
        }

        UpdateAuth(response.Result);

        await RefreshUserInfo(cancellationToken);

        return new HttpResponse<string>(idToken, response);
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
        var tokenResponse = await GetFreshToken(cancellationToken);
        if (tokenResponse is HttpResponse<string> httpResponse &&
            httpResponse.IsError)
        {
            return new(httpResponse);
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

        return await HttpHelpers.ExecuteWithContent(App.GetClient(), stream, HttpMethod.Post, AuthenticationApi.GoogleGetConfirmationCodeUrl, cancellationToken);
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

        var tokenResponse = await GetFreshToken(cancellationToken);
        if (tokenResponse is HttpResponse<string> httpResponse &&
            httpResponse.IsError)
        {
            return new(httpResponse);
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

        var response = await HttpHelpers.ExecuteWithContent(App.GetClient(), stream, HttpMethod.Post, AuthenticationApi.GoogleUpdateUser, cancellationToken);
        if (response.IsError)
        {
            return response;
        }

        var responseRefresh = await RefreshUserInfo(cancellationToken);
        if (responseRefresh.IsError)
        {
            return responseRefresh;
        }

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

        var tokenResponse = await GetFreshToken(cancellationToken);
        if (tokenResponse is HttpResponse<string> httpResponse &&
            httpResponse.IsError)
        {
            return new(httpResponse);
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

        var response = await HttpHelpers.ExecuteWithContent(App.GetClient(), stream, HttpMethod.Post, AuthenticationApi.GoogleUpdateUser, cancellationToken);
        if (response.IsError)
        {
            return response;
        }

        var responseRefresh = await RefreshUserInfo(cancellationToken);
        if (responseRefresh.IsError)
        {
            return responseRefresh;
        }

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
    public async Task<HttpResponse> UpdateProfile(string displayName, string photoUrl, CancellationToken cancellationToken = default)
    {
        var tokenResponse = await GetFreshToken(cancellationToken);
        if (tokenResponse is HttpResponse<string> httpResponse &&
            httpResponse.IsError)
        {
            return new(httpResponse);
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

        var response = await HttpHelpers.ExecuteWithContent(App.GetClient(), stream, HttpMethod.Post, AuthenticationApi.GoogleSetAccountUrl, cancellationToken);
        if (response.IsError)
        {
            return response;
        }

        var responseRefresh = await RefreshUserInfo(cancellationToken);
        if (responseRefresh.IsError)
        {
            return responseRefresh;
        }

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
        var tokenResponse = await GetFreshToken(cancellationToken);
        if (tokenResponse is HttpResponse<string> httpResponse &&
            httpResponse.IsError)
        {
            return new(httpResponse);
        }

        using MemoryStream stream = new();
        Utf8JsonWriter writer = new(stream);

        writer.WriteStartObject();
        writer.WritePropertyName("idToken");
        writer.WriteStringValue(tokenResponse.Result);
        writer.WriteEndObject();

        return await HttpHelpers.ExecuteWithContent(App.GetClient(), stream, HttpMethod.Post, AuthenticationApi.GoogleDeleteUserUrl, cancellationToken);
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

        var tokenResponse = await GetFreshToken(cancellationToken);
        if (tokenResponse is HttpResponse<string> httpResponse &&
            httpResponse.IsError)
        {
            return new(httpResponse);
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

        var response = await HttpHelpers.ExecuteWithContent(App.GetClient(), stream, HttpMethod.Post, AuthenticationApi.GoogleSetAccountUrl, cancellationToken);
        if (response.IsError)
        {
            return response;
        }

        var responseRefresh = await RefreshUserInfo(cancellationToken);
        if (responseRefresh.IsError)
        {
            return responseRefresh;
        }

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

        var tokenResponse = await GetFreshToken(cancellationToken);
        if (tokenResponse is HttpResponse<string> httpResponse &&
            httpResponse.IsError)
        {
            return new(httpResponse);
        }

        var providerId = AuthenticationApi.GetProviderId(authType);

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

        var response = await HttpHelpers.ExecuteWithContent(App.GetClient(), stream, HttpMethod.Post, AuthenticationApi.GoogleIdentityUrl, cancellationToken);
        if (response.IsError)
        {
            return response;
        }

        var responseRefresh = await RefreshUserInfo(cancellationToken);
        if (responseRefresh.IsError)
        {
            return responseRefresh;
        }

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
        var tokenResponse = await GetFreshToken(cancellationToken);
        if (tokenResponse is HttpResponse<string> httpResponse &&
            httpResponse.IsError)
        {
            return new(httpResponse);
        }

        string? providerId;
        if (authType == FirebaseAuthType.EmailAndPassword)
        {
            providerId = authType.ToEnumString();
        }
        else
        {
            providerId = AuthenticationApi.GetProviderId(authType);
        }

        using MemoryStream stream = new();
        Utf8JsonWriter writer = new(stream);

        writer.WriteStartObject();
        writer.WritePropertyName("deleteProvider");
        writer.WriteStringValue(providerId);
        writer.WritePropertyName("idToken");
        writer.WriteStringValue(tokenResponse.Result);
        writer.WriteEndObject();

        var response = await HttpHelpers.ExecuteWithContent(App.GetClient(), stream, HttpMethod.Post, AuthenticationApi.GoogleSetAccountUrl, cancellationToken);
        if (response.IsError)
        {
            return response;
        }

        var responseRefresh = await RefreshUserInfo(cancellationToken);
        if (responseRefresh.IsError)
        {
            return responseRefresh;
        }

        return response;
    }
}
