using System.Threading.Tasks;
using RestfulFirebase.Authentication.Internals;
using RestfulFirebase.Common.Http;
using RestfulFirebase.Authentication.Models;
using System.Threading;
using System.IO;
using System.Text.Json;
using RestfulFirebase.Authentication.Enums;
using System.Diagnostics.CodeAnalysis;

namespace RestfulFirebase.Authentication;

public partial class AuthenticationApi
{
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
    public FirebaseUser Decrypt(string data, params int[] pattern)
    {
        return FirebaseUser.Decrypt(App, data, pattern);
    }

    /// <summary>
    /// Request to get the reCaptcha site key to be used for sending verification code to a phone number.
    /// </summary>
    /// <param name="cancellationToken">
    /// The <see cref="CancellationToken"/> that propagates notification if the operations should be canceled.
    /// </param>
    /// <returns>
    /// The <see cref="Task"/> proxy that represents the <see cref="HttpResponse"/> with the reCaptcha site key <see cref="string"/>.
    /// </returns>
#if NET5_0_OR_GREATER
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(RecaptchaSiteKeyDefinition))]
    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
#endif
    public async Task<HttpResponse<string>> GetRecaptchaSiteKey(CancellationToken cancellationToken = default)
    {
        HttpResponse<string> response = new();

        var getResponse = await ExecuteGet<RecaptchaSiteKeyDefinition>(GoogleRecaptchaParams, cancellationToken);
        response.Append(getResponse);
        if (getResponse.IsError || getResponse.Result.RecaptchaSiteKey == null)
        {
            return response;
        }

        response.Append(getResponse.Result.RecaptchaSiteKey);

        return response;
    }

    /// <summary>
    /// Request to send a verification code to a phone number.
    /// </summary>
    /// <param name="phoneNumber">
    /// The phone number to send verification code.
    /// </param>
    /// <param name="recaptchaToken">
    /// The recaptcha token from Google reCaptcha.
    /// </param>
    /// <param name="cancellationToken">
    /// The <see cref="CancellationToken"/> that propagates notification if the operations should be canceled.
    /// </param>
    /// <returns>
    /// The <see cref="Task"/> proxy that represents the <see cref="HttpResponse"/> with the session info <see cref="string"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="phoneNumber"/> or
    /// <paramref name="recaptchaToken"/> is a null reference.
    /// </exception>
#if NET5_0_OR_GREATER
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(SessionInfoDefinition))]
    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
#endif
    public async Task<HttpResponse<string>> SendVerificationCode(string phoneNumber, string recaptchaToken, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(phoneNumber);
        ArgumentNullException.ThrowIfNull(recaptchaToken);

        using MemoryStream stream = new();
        Utf8JsonWriter writer = new(stream);

        writer.WriteStartObject();
        writer.WritePropertyName("phoneNumber");
        writer.WriteStringValue(phoneNumber);
        writer.WritePropertyName("recaptchaToken");
        writer.WriteStringValue(recaptchaToken);
        writer.WriteEndObject();

        await writer.FlushAsync(cancellationToken);

        HttpResponse<string> response = new();

        var postResponse = await ExecutePost<SessionInfoDefinition>(stream, GoogleSendVerificationCode, cancellationToken);
        response.Append(postResponse);
        if (postResponse.IsError || postResponse.Result.SessionInfo == null)
        {
            return response;
        }

        response.Append(postResponse.Result.SessionInfo);

        return response;
    }

    /// <summary>
    /// Request to send password reset email to the existing account provided with the email.
    /// </summary>
    /// <param name="email">
    /// The email of the request.
    /// </param>
    /// <param name="cancellationToken">
    /// The <see cref="CancellationToken"/> that propagates notification if the operations should be canceled.
    /// </param>
    /// <returns>
    /// The <see cref="Task"/> proxy that represents the <see cref="HttpResponse"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="email"/> is a null reference.
    /// </exception>
    public async Task<HttpResponse> SendPasswordResetEmail(string email, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(email);

        using MemoryStream stream = new();
        Utf8JsonWriter writer = new(stream);

        writer.WriteStartObject();
        writer.WritePropertyName("requestType");
        writer.WriteStringValue("PASSWORD_RESET");
        writer.WritePropertyName("email");
        writer.WriteStringValue(email);
        writer.WriteEndObject();

        await writer.FlushAsync(cancellationToken);

        return await ExecutePost(stream, GoogleGetConfirmationCodeUrl, cancellationToken);
    }

    /// <summary>
    /// Request to get all linked accounts of the user.
    /// </summary>
    /// <param name="email">
    /// The email of the request.
    /// </param>
    /// <param name="cancellationToken">
    /// The <see cref="CancellationToken"/> that propagates notification if the operations should be canceled.
    /// </param>
    /// <returns>
    /// The <see cref="Task"/> proxy that represents the <see cref="HttpResponse"/> with the result <see cref="ProviderQuery"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="email"/> is a null reference.
    /// </exception>
#if NET5_0_OR_GREATER
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(ProviderQuery))]
    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
#endif
    public async Task<HttpResponse<ProviderQuery>> GetLinkedAccounts(string email, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(email);

        using MemoryStream stream = new();
        Utf8JsonWriter writer = new(stream);

        writer.WriteStartObject();
        writer.WritePropertyName("identifier");
        writer.WriteStringValue(email);
        writer.WritePropertyName("continueUri");
        writer.WriteStringValue("http://localhost");
        writer.WriteEndObject();

        await writer.FlushAsync(cancellationToken);

        var response = await ExecutePost<ProviderQuery>(stream, GoogleCreateAuthUrl, cancellationToken);
        if (response.IsError)
        {
            return response;
        }

        response.Result.Email = email;

        return response;
    }

    /// <summary>
    /// Request to creates user with the provided email and password.
    /// </summary>
    /// <param name="email">
    /// The email of the request.
    /// </param>
    /// <param name="password">
    /// The password of the request.
    /// </param>
    /// <param name="sendVerificationEmail">
    /// Sends email verification after user creation if specified <c>true</c>; otherwise, <c>false</c>.
    /// </param>
    /// <param name="cancellationToken">
    /// The <see cref="CancellationToken"/> that propagates notification if the operations should be canceled.
    /// </param>
    /// <returns>
    /// The <see cref="Task"/> proxy that represents the <see cref="HttpResponse"/> with the result <see cref="FirebaseUser"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="email"/> or
    /// <paramref name="password"/> is a null reference.
    /// </exception>
#if NET5_0_OR_GREATER
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(FirebaseAuth))]
    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
#endif
    public async Task<HttpResponse<FirebaseUser>> CreateUserWithEmailAndPassword(string email, string password, bool sendVerificationEmail = true, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(email);
        ArgumentNullException.ThrowIfNull(password);

        using MemoryStream stream = new();
        Utf8JsonWriter writer = new(stream);

        writer.WriteStartObject();
        writer.WritePropertyName("email");
        writer.WriteStringValue(email);
        writer.WritePropertyName("password");
        writer.WriteStringValue(password);
        writer.WritePropertyName("returnSecureToken");
        writer.WriteBooleanValue(true);
        writer.WriteEndObject();

        await writer.FlushAsync(cancellationToken);

        HttpResponse<FirebaseUser> response = new();

        var startResponse = await StartUser(stream, GoogleSignUpUrl, cancellationToken);
        response.Append(startResponse);
        if (startResponse.IsError)
        {
            return response;
        }

        if (sendVerificationEmail)
        {
            var sendVerificationResponse = await startResponse.Result.SendEmailVerification(cancellationToken);
            response.Append(sendVerificationResponse);
            if (sendVerificationResponse.IsError)
            {
                return response;
            }
        }

        return response;
    }

    /// <summary>
    /// Request to sign in with provided email and password.
    /// </summary>
    /// <param name="email">
    /// The email of the request.
    /// </param>
    /// <param name="password">
    /// The password of the request.
    /// </param>
    /// <param name="tenantId">
    /// The account tenant id of the user.
    /// </param>
    /// <param name="cancellationToken">
    /// The <see cref="CancellationToken"/> that propagates notification if the operations should be canceled.
    /// </param>
    /// <returns>
    /// The <see cref="Task"/> proxy that represents the <see cref="HttpResponse"/> with the result <see cref="FirebaseUser"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="email"/> or
    /// <paramref name="password"/> is a null reference.
    /// </exception>
    public async Task<HttpResponse<FirebaseUser>> SignInWithEmailAndPassword(string email, string password, string? tenantId = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(email);
        ArgumentNullException.ThrowIfNull(password);

        using MemoryStream stream = new();
        Utf8JsonWriter writer = new(stream);

        writer.WriteStartObject();
        writer.WritePropertyName("email");
        writer.WriteStringValue(email);
        writer.WritePropertyName("password");
        writer.WriteStringValue(password);
        if (tenantId != null)
        {
            writer.WritePropertyName("tenantId");
            writer.WriteStringValue(tenantId);
        }
        writer.WritePropertyName("returnSecureToken");
        writer.WriteBooleanValue(true);
        writer.WriteEndObject();

        await writer.FlushAsync(cancellationToken);

        return await StartUser(stream, GooglePasswordUrl, cancellationToken);
    }

    /// <summary>
    /// Request to sign in a phone number with the provided sessionInfo and code from reCaptcha validation and sms OTP message.
    /// </summary>
    /// <param name="sessionInfo">
    /// The session info token returned from <see cref="SendVerificationCode(string, string, CancellationToken)"/>.
    /// </param>
    /// <param name="code">
    /// The phone sms OTP code.
    /// </param>
    /// <param name="cancellationToken">
    /// The <see cref="CancellationToken"/> that propagates notification if the operations should be canceled.
    /// </param>
    /// <returns>
    /// The <see cref="Task"/> proxy that represents the <see cref="HttpResponse"/> with the result <see cref="FirebaseUser"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="sessionInfo"/> or
    /// <paramref name="code"/> is a null reference.
    /// </exception>
    public async Task<HttpResponse<FirebaseUser>> SignInWithPhoneNumber(string sessionInfo, string code, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(sessionInfo);
        ArgumentNullException.ThrowIfNull(code);

        using MemoryStream stream = new();
        Utf8JsonWriter writer = new(stream);

        writer.WriteStartObject();
        writer.WritePropertyName("sessionInfo");
        writer.WriteStringValue(sessionInfo);
        writer.WritePropertyName("code");
        writer.WriteStringValue(code);
        writer.WritePropertyName("returnSecureToken");
        writer.WriteBooleanValue(true);
        writer.WriteEndObject();

        await writer.FlushAsync(cancellationToken);

        return await StartUser(stream, GoogleSignInWithPhoneNumber, cancellationToken);
    }

    /// <summary>
    /// Request to sign in with custom token provided by firebase.
    /// </summary>
    /// <param name="customToken">
    /// The token provided by firebase.
    /// </param>
    /// <param name="cancellationToken">
    /// The <see cref="CancellationToken"/> that propagates notification if the operations should be canceled.
    /// </param>
    /// <returns>
    /// The <see cref="Task"/> proxy that represents the <see cref="HttpResponse"/> with the result <see cref="FirebaseUser"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="customToken"/> is a null reference.
    /// </exception>
    public async Task<HttpResponse<FirebaseUser>> SignInWithCustomToken(string customToken, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(customToken);

        using MemoryStream stream = new();
        Utf8JsonWriter writer = new(stream);

        writer.WriteStartObject();
        writer.WritePropertyName("token");
        writer.WriteStringValue(customToken);
        writer.WritePropertyName("returnSecureToken");
        writer.WriteBooleanValue(true);
        writer.WriteEndObject();

        await writer.FlushAsync(cancellationToken);

        return await StartUser(stream, GoogleCustomAuthUrl, cancellationToken);
    }

    /// <summary>
    /// Request to sign in with oauth provided with auth type and oauth token.
    /// </summary>
    /// <param name="authType">
    /// The <see cref="FirebaseAuthType"/> of the oauth used.
    /// </param>
    /// <param name="oauthToken">
    /// The token of the provided oauth type.
    /// </param>
    /// <param name="cancellationToken">
    /// The <see cref="CancellationToken"/> that propagates notification if the operations should be canceled.
    /// </param>
    /// <returns>
    /// The <see cref="Task"/> proxy that represents the <see cref="HttpResponse"/> with the result <see cref="FirebaseUser"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="oauthToken"/> is a null reference.
    /// </exception>
    public async Task<HttpResponse<FirebaseUser>> SignInWithOAuth(FirebaseAuthType authType, string oauthToken, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(oauthToken);

        var providerId = GetProviderId(authType);

        using MemoryStream stream = new();
        Utf8JsonWriter writer = new(stream);

        writer.WriteStartObject();
        writer.WritePropertyName("postBody");
        writer.WriteStringValue($"{(authType == FirebaseAuthType.Apple ? "id_token" : "access_token")}={oauthToken}&providerId={providerId}");
        writer.WritePropertyName("requestUri");
        writer.WriteStringValue("http://localhost");
        writer.WritePropertyName("returnSecureToken");
        writer.WriteBooleanValue(true);
        writer.WriteEndObject();

        await writer.FlushAsync(cancellationToken);

        return await StartUser(stream, GoogleIdentityUrl, cancellationToken);
    }

    /// <summary>
    /// Request to sign in with twitter oauth token provided with oauth access token and oauth access secret from twitter.
    /// </summary>
    /// <param name="oauthAccessToken">
    /// The access token provided by twitter.
    /// </param>
    /// <param name="oauthTokenSecret">
    /// The the oauth token secret provided by twitter.
    /// </param>
    /// <param name="cancellationToken">
    /// The <see cref="CancellationToken"/> that propagates notification if the operations should be canceled.
    /// </param>
    /// <returns>
    /// The <see cref="Task"/> proxy that represents the <see cref="HttpResponse"/> with the result <see cref="FirebaseUser"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="oauthAccessToken"/> or
    /// <paramref name="oauthTokenSecret"/> is a null reference.
    /// </exception>
    public async Task<HttpResponse<FirebaseUser>> SignInWithOAuthTwitterToken(string oauthAccessToken, string oauthTokenSecret, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(oauthAccessToken);
        ArgumentNullException.ThrowIfNull(oauthTokenSecret);

        var providerId = GetProviderId(FirebaseAuthType.Twitter);

        using MemoryStream stream = new();
        Utf8JsonWriter writer = new(stream);

        writer.WriteStartObject();
        writer.WritePropertyName("postBody");
        writer.WriteStringValue($"access_token={oauthAccessToken}&oauth_token_secret={oauthTokenSecret}&providerId={providerId}");
        writer.WritePropertyName("requestUri");
        writer.WriteStringValue("http://localhost");
        writer.WritePropertyName("returnSecureToken");
        writer.WriteBooleanValue(true);
        writer.WriteEndObject();

        await writer.FlushAsync(cancellationToken);

        return await StartUser(stream, GoogleIdentityUrl, cancellationToken);
    }

    /// <summary>
    /// Request to sign in with google id token.
    /// </summary>
    /// <param name="idToken">
    /// The ID token provided by google.
    /// </param>
    /// <param name="cancellationToken">
    /// The <see cref="CancellationToken"/> that propagates notification if the operations should be canceled.
    /// </param>
    /// <returns>
    /// The <see cref="Task"/> proxy that represents the <see cref="HttpResponse"/> with the result <see cref="FirebaseUser"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="idToken"/> is a null reference.
    /// </exception>
    public async Task<HttpResponse<FirebaseUser>> SignInWithGoogleIdToken(string idToken, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(idToken);

        var providerId = GetProviderId(FirebaseAuthType.Google);

        using MemoryStream stream = new();
        Utf8JsonWriter writer = new(stream);

        writer.WriteStartObject();
        writer.WritePropertyName("postBody");
        writer.WriteStringValue($"id_token={idToken}&providerId={providerId}");
        writer.WritePropertyName("requestUri");
        writer.WriteStringValue("http://localhost");
        writer.WritePropertyName("returnSecureToken");
        writer.WriteBooleanValue(true);
        writer.WriteEndObject();

        await writer.FlushAsync(cancellationToken);

        return await StartUser(stream, GoogleIdentityUrl, cancellationToken);
    }

    /// <summary>
    /// Request to sign in anonimously.
    /// </summary>
    /// <param name="cancellationToken">
    /// The <see cref="CancellationToken"/> that propagates notification if the operations should be canceled.
    /// </param>
    /// <returns>
    /// The <see cref="Task"/> proxy that represents the <see cref="HttpResponse"/> with the result <see cref="FirebaseUser"/>.
    /// </returns>
    public async Task<HttpResponse<FirebaseUser>> SignInAnonymously(CancellationToken cancellationToken = default)
    {
        using MemoryStream stream = new();
        Utf8JsonWriter writer = new(stream);

        writer.WriteStartObject();
        writer.WritePropertyName("returnSecureToken");
        writer.WriteBooleanValue(true);
        writer.WriteEndObject();

        await writer.FlushAsync(cancellationToken);

        return await StartUser(stream, GoogleSignUpUrl, cancellationToken);
    }
}
