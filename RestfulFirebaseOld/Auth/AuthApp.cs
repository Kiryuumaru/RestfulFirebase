﻿using RestfulFirebase.Utilities;
using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RestfulFirebase.Exceptions;
using System.Text.Json;
using DisposableHelpers.Attributes;

namespace RestfulFirebase.Auth;

/// <summary>
/// App module that provides firebase authentication implementations.
/// </summary>
[Disposable]
public partial class AuthApp
{
    #region Properties

    /// <summary>
    /// Gets the <see cref="RestfulFirebaseApp"/> used by this instance.
    /// </summary>
    public RestfulFirebaseApp App { get; private set; }

    /// <summary>
    /// Gets the <see cref="Auth.Session"/> of the authenticated token. Returns <c>null</c> if no authenticated token is present.
    /// </summary>
    public Session? Session => session.Exist ? session : null;

    /// <summary>
    /// Gets <c>true</c> whether the user is authenticated; otherwise <c>false</c>.
    /// </summary>
    public bool IsAuthenticated => session.Exist;

    /// <summary>
    /// Event raised on the current context when the authentication is refreshed.
    /// </summary>
    public event EventHandler? AuthRefreshed;

    /// <summary>
    /// Event raised on the current context when the module is authenticated.
    /// </summary>
    public event EventHandler? Authenticated;

    /// <summary>
    /// Event raised on the current context when the module is unauthenticated.
    /// </summary>
    public event EventHandler? Unauthenticated;

    /// <summary>
    /// Event raised on the current context when the module is unauthenticated.
    /// </summary>
    public event EventHandler<AuthenticationChangesEventArgs>? AuthenticationChanges;

    internal const string GoogleSignInWithPhoneNumber = "https://identitytoolkit.googleapis.com/v1/accounts:signInWithPhoneNumber?key={0}";
    internal const string GoogleRecaptchaParams = "https://identitytoolkit.googleapis.com/v1/recaptchaParams?key={0}";
    internal const string GoogleSendVerificationCode = "https://identitytoolkit.googleapis.com/v1/accounts:sendVerificationCode?key={0}";
    internal const string GoogleRefreshAuth = "https://securetoken.googleapis.com/v1/token?key={0}";
    internal const string GoogleCustomAuthUrl = "https://www.googleapis.com/identitytoolkit/v3/relyingparty/verifyCustomToken?key={0}";
    internal const string GoogleGetUser = "https://www.googleapis.com/identitytoolkit/v3/relyingparty/getAccountInfo?key={0}";
    internal const string GoogleIdentityUrl = "https://www.googleapis.com/identitytoolkit/v3/relyingparty/verifyAssertion?key={0}";
    internal const string GoogleSignUpUrl = "https://www.googleapis.com/identitytoolkit/v3/relyingparty/signupNewUser?key={0}";
    internal const string GooglePasswordUrl = "https://www.googleapis.com/identitytoolkit/v3/relyingparty/verifyPassword?key={0}";
    internal const string GoogleDeleteUserUrl = "https://www.googleapis.com/identitytoolkit/v3/relyingparty/deleteAccount?key={0}";
    internal const string GoogleGetConfirmationCodeUrl = "https://www.googleapis.com/identitytoolkit/v3/relyingparty/getOobConfirmationCode?key={0}";
    internal const string GoogleSetAccountUrl = "https://www.googleapis.com/identitytoolkit/v3/relyingparty/setAccountInfo?key={0}";
    internal const string GoogleCreateAuthUrl = "https://www.googleapis.com/identitytoolkit/v3/relyingparty/createAuthUri?key={0}";
    internal const string GoogleUpdateUserPassword = "https://identitytoolkit.googleapis.com/v1/accounts:update?key={0}";
    internal const string ProfileDeleteDisplayName = "DISPLAY_NAME";
    internal const string ProfileDeletePhotoUrl = "PHOTO_URL";

    private HttpClient? client;
    private readonly Session session;

#endregion

    #region Initializers

    internal AuthApp(RestfulFirebaseApp app)
    {
        App = app;
        session = new Session(App);
    }

    #endregion

    #region Helpers

    internal static string? GetProviderId(FirebaseAuthType authType)
    {
        return authType switch
        {
            FirebaseAuthType.Facebook or
            FirebaseAuthType.Google or
            FirebaseAuthType.Apple or
            FirebaseAuthType.Github or
            FirebaseAuthType.Twitter => authType.ToEnumString(),
            FirebaseAuthType.EmailAndPassword => throw new InvalidOperationException("Email auth type cannot be used like this. Use methods specific to email & password authentication."),
            _ => throw new NotImplementedException(""),
        };
    }

    internal HttpClient GetClient()
    {
        client ??= App.Config.HttpClientFactory.GetHttpClient();

        return client;
    }

    internal async Task<string> ExecuteWithGet(string googleUrl)
    {
        string responseData = "N/A";

        try
        {
            var response = await GetClient().GetAsync(
                new Uri(string.Format(googleUrl, App.Config.ApiKey)),
                new CancellationTokenSource(App.Config.AuthRequestTimeout).Token).ConfigureAwait(false);
            responseData = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            response.EnsureSuccessStatusCode();

            return responseData;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw ExceptionHelpers.GetException(responseData, ex);
        }
    }

    internal async Task<string> ExecuteWithPostContent(string googleUrl, string postContent)
    {
        string responseData = "N/A";

        try
        {
            var response = await GetClient().PostAsync(
                new Uri(string.Format(googleUrl, App.Config.ApiKey)),
                new StringContent(postContent, Encoding.UTF8, "Application/json"),
                new CancellationTokenSource(App.Config.AuthRequestTimeout).Token).ConfigureAwait(false);
            responseData = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            response.EnsureSuccessStatusCode();

            return responseData;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw ExceptionHelpers.GetException(responseData, ex);
        }
    }

    internal async Task<FirebaseAuth> ExecuteAuthWithPostContent(string googleUrl, string postContent)
    {
        string responseData = await ExecuteWithPostContent(googleUrl, postContent);
        var user = JsonSerializer.Deserialize<User>(responseData, RestfulFirebaseApp.DefaultJsonSerializerOption);
        var auth = JsonSerializer.Deserialize<FirebaseAuth>(responseData, RestfulFirebaseApp.DefaultJsonSerializerOption);

        if (user == null || auth == null)
        {
            throw new AuthUndefinedException();
        }

        auth.User = user;

        session.UpdateAuth(auth);

        return auth;
    }

    internal async Task RefreshUserInfo(FirebaseAuth auth)
    {
        var responseData = "N/A";

        try
        {
            var content = $"{{\"idToken\":\"{auth.FirebaseToken}\"}}";
            var response = await GetClient().PostAsync(
                new Uri(string.Format(GoogleGetUser, App.Config.ApiKey)),
                new StringContent(content, Encoding.UTF8, "Application/json"),
                new CancellationTokenSource(App.Config.AuthRequestTimeout).Token).ConfigureAwait(false);
            responseData = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            var resultJson = JsonDocument.Parse(responseData);
            if (!(resultJson?.RootElement.TryGetProperty("users", out JsonElement userJson) ?? false))
            {
                throw new AuthUndefinedException();
            }
            var user = JsonSerializer.Deserialize<User>(userJson.EnumerateArray().First());

            if (user == null)
            {
                throw new AuthUndefinedException();
            }

            auth.User = user;

            session.UpdateAuth(auth);

            OnAuthRefreshed();
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw ExceptionHelpers.GetException(responseData, ex);
        }
    }

    #endregion

    #region Methods

    /// <summary>
    /// Gets the reCaptcha site key to be used for sending verification code to a phone number.
    /// </summary>
    /// <returns>
    /// The <see cref="Task"/> proxy that represents the sessioninfo of the verification sent.
    /// </returns>
    public async Task<string> GetRecaptchaSiteKey()
    {
        var responseData = await ExecuteWithGet(GoogleRecaptchaParams).ConfigureAwait(false);

        var definition = new { recaptchaSiteKey = "" };

        var response = JsonSerializerExtensions.DeserializeAnonymousType(responseData, definition, RestfulFirebaseApp.DefaultJsonSerializerOption);
        
        if (response == null)
        {
            throw new Exception();
        }

        return response.recaptchaSiteKey;
    }

    /// <summary>
    /// Send a verification code to a phone number.
    /// </summary>
    /// <param name="phoneNumber">
    /// The phone number to send verification code.
    /// </param>
    /// <param name="recaptchaToken">
    /// The recaptcha token from Google reCaptcha.
    /// </param>
    /// <returns>
    /// The <see cref="Task"/> proxy of the specified task.
    /// </returns>
    public async Task<string> SendVerificationCode(string phoneNumber, string recaptchaToken)
    {
        string content = $"{{\"phoneNumber\":\"{phoneNumber}\",\"recaptchaToken\":\"{recaptchaToken}\"}}";
        
        var responseData = await ExecuteWithPostContent(GoogleSendVerificationCode, content).ConfigureAwait(false);

        var definition = new { sessionInfo = "" };

        var response = JsonSerializerExtensions.DeserializeAnonymousType(responseData, definition, RestfulFirebaseApp.DefaultJsonSerializerOption);

        if (response == null)
        {
            throw new Exception();
        }

        return response.sessionInfo;
    }

    /// <summary>
    /// Creates user with the provided <paramref name="email"/> and <paramref name="password"/>.
    /// </summary>
    /// <param name="email">
    /// The email of the user.
    /// </param>
    /// <param name="password">
    /// The password of the user.
    /// </param>
    /// <param name="sendVerificationEmail">
    /// <c>true</c> to send email verification after user creation; otherwise, <c>false</c>.
    /// </param>
    /// <returns>
    /// The <see cref="Task"/> proxy of the specified task.
    /// </returns>
    /// <exception cref="AuthEmailExistsException">
    /// The email address is already in use by another account.
    /// </exception>
    /// <exception cref="AuthWeakPasswordException">
    /// The password must be 6 characters long or more.
    /// </exception>
    /// <exception cref="AuthOperationNotAllowedException">
    /// Password sign-in is disabled for this project.
    /// </exception>
    /// <exception cref="AuthTooManyAttemptsException">
    /// There is an unusual activity on device.
    /// </exception>
    /// <exception cref="AuthAPIKeyNotValidException">
    /// API key not valid. Please pass a valid API key.
    /// </exception>
    /// <exception cref="AuthInvalidIDTokenException">
    /// The user's credential is no longer valid. The user must sign in again.
    /// </exception>
    /// <exception cref="AuthUserNotFoundException">
    /// There is no user record corresponding to this identifier. The user may have been deleted.
    /// </exception>
    /// <exception cref="AuthUndefinedException">
    /// The error occured is undefined.
    /// </exception>
    /// <exception cref="OperationCanceledException">
    /// The operation was cancelled.
    /// </exception>
    public async Task CreateUserWithEmailAndPassword(string email, string password, bool sendVerificationEmail = false)
    {
        var content = $"{{\"email\":\"{email}\",\"password\":\"{password}\",\"returnSecureToken\":true}}";

        var auth = await ExecuteAuthWithPostContent(GoogleSignUpUrl, content).ConfigureAwait(false);

        await RefreshUserInfo(auth).ConfigureAwait(false);

        InvokeAuthenticationEvents();

        if (sendVerificationEmail)
        {
            await session.SendEmailVerification().ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Sign in with custom token provided by firebase.
    /// </summary>
    /// <param name="customToken">
    /// The token provided by firebase.
    /// </param>
    /// <returns>
    /// The <see cref="Task"/> proxy of the specified task.
    /// </returns>
    /// <exception cref="AuthInvalidCustomTokenException">
    /// The custom token format is incorrect or the token is invalid for some reason (e.g. expired, invalid signature etc.)
    /// </exception>
    /// <exception cref="AuthCredentialMismatchException">
    /// The custom token corresponds to a different Firebase project.
    /// </exception>
    /// <exception cref="AuthAPIKeyNotValidException">
    /// API key not valid. Please pass a valid API key.
    /// </exception>
    /// <exception cref="AuthUndefinedException">
    /// The error occured is undefined.
    /// </exception>
    /// <exception cref="OperationCanceledException">
    /// The operation was cancelled.
    /// </exception>
    public async Task SignInWithCustomToken(string customToken)
    {
        string content = $"{{\"token\":\"{customToken}\",\"returnSecureToken\":true}}";

        var auth = await ExecuteAuthWithPostContent(GoogleCustomAuthUrl, content).ConfigureAwait(false);

        await RefreshUserInfo(auth).ConfigureAwait(false);

        InvokeAuthenticationEvents();
    }

    /// <summary>
    /// Sign in with oauth provided with <paramref name="authType"/> and <paramref name="oauthToken"/>.
    /// </summary>
    /// <param name="authType">
    /// The <see cref="FirebaseAuthType"/> of the oauth used.
    /// </param>
    /// <param name="oauthToken">
    /// The token of the provided <paramref name="authType"/> type.
    /// </param>
    /// <returns>
    /// The <see cref="Task"/> proxy of the specified task.
    /// </returns>
    /// <exception cref="AuthOperationNotAllowedException">
    /// The corresponding provider is disabled for this project.
    /// </exception>
    /// <exception cref="AuthInvalidIDPResponseException">
    /// The supplied auth credential is malformed or has expired.
    /// </exception>
    /// <exception cref="AuthAPIKeyNotValidException">
    /// API key not valid. Please pass a valid API key.
    /// </exception>
    /// <exception cref="AuthInvalidIDTokenException">
    /// The user's credential is no longer valid. The user must sign in again.
    /// </exception>
    /// <exception cref="AuthUserNotFoundException">
    /// There is no user record corresponding to this identifier. The user may have been deleted.
    /// </exception>
    /// <exception cref="AuthUndefinedException">
    /// The error occured is undefined.
    /// </exception>
    /// <exception cref="OperationCanceledException">
    /// The operation was cancelled.
    /// </exception>
    public async Task SignInWithOAuth(FirebaseAuthType authType, string oauthToken)
    {
        var providerId = GetProviderId(authType);

        string content = authType switch
        {
            FirebaseAuthType.Apple => $"{{\"postBody\":\"id_token={oauthToken}&providerId={providerId}\",\"requestUri\":\"http://localhost\",\"returnSecureToken\":true}}",
            _ => $"{{\"postBody\":\"access_token={oauthToken}&providerId={providerId}\",\"requestUri\":\"http://localhost\",\"returnSecureToken\":true}}",
        };

        var auth = await ExecuteAuthWithPostContent(GoogleIdentityUrl, content).ConfigureAwait(false);

        await RefreshUserInfo(auth).ConfigureAwait(false);

        InvokeAuthenticationEvents();
    }

    /// <summary>
    /// Sign in with twitter oauth token provided with <paramref name="oauthAccessToken"/> and <paramref name="oauthTokenSecret"/> from twitter.
    /// </summary>
    /// <param name="oauthAccessToken">
    /// The access token provided by twitter.
    /// </param>
    /// <param name="oauthTokenSecret">
    /// The oauth token secret provided by twitter
    /// </param>
    /// <returns>
    /// The <see cref="Task"/> proxy of the specified task.
    /// </returns>
    /// <exception cref="AuthOperationNotAllowedException">
    /// The corresponding provider is disabled for this project.
    /// </exception>
    /// <exception cref="AuthInvalidIDPResponseException">
    /// The supplied auth credential is malformed or has expired.
    /// </exception>
    /// <exception cref="AuthAPIKeyNotValidException">
    /// API key not valid. Please pass a valid API key.
    /// </exception>
    /// <exception cref="AuthInvalidIDTokenException">
    /// The user's credential is no longer valid. The user must sign in again.
    /// </exception>
    /// <exception cref="AuthUserNotFoundException">
    /// There is no user record corresponding to this identifier. The user may have been deleted.
    /// </exception>
    /// <exception cref="AuthUndefinedException">
    /// The error occured is undefined.
    /// </exception>
    /// <exception cref="OperationCanceledException">
    /// The operation was cancelled.
    /// </exception>
    public async Task SignInWithOAuthTwitterToken(string oauthAccessToken, string oauthTokenSecret)
    {
        var providerId = GetProviderId(FirebaseAuthType.Twitter);
        var content = $"{{\"postBody\":\"access_token={oauthAccessToken}&oauth_token_secret={oauthTokenSecret}&providerId={providerId}\",\"requestUri\":\"http://localhost\",\"returnSecureToken\":true}}";

        var auth = await ExecuteAuthWithPostContent(GoogleIdentityUrl, content).ConfigureAwait(false);

        await RefreshUserInfo(auth).ConfigureAwait(false);

        InvokeAuthenticationEvents();
    }

    /// <summary>
    /// Sign in with google id token.
    /// </summary>
    /// <param name="idToken">
    /// The id token provided by google.
    /// </param>
    /// <returns>
    /// The <see cref="Task"/> proxy of the specified task.
    /// </returns>
    /// <exception cref="AuthOperationNotAllowedException">
    /// The corresponding provider is disabled for this project.
    /// </exception>
    /// <exception cref="AuthInvalidIDPResponseException">
    /// The supplied auth credential is malformed or has expired.
    /// </exception>
    /// <exception cref="AuthAPIKeyNotValidException">
    /// API key not valid. Please pass a valid API key.
    /// </exception>
    /// <exception cref="AuthInvalidIDTokenException">
    /// The user's credential is no longer valid. The user must sign in again.
    /// </exception>
    /// <exception cref="AuthUserNotFoundException">
    /// There is no user record corresponding to this identifier. The user may have been deleted.
    /// </exception>
    /// <exception cref="AuthUndefinedException">
    /// The error occured is undefined.
    /// </exception>
    /// <exception cref="OperationCanceledException">
    /// The operation was cancelled.
    /// </exception>
    public async Task SignInWithGoogleIdToken(string idToken)
    {
        var providerId = GetProviderId(FirebaseAuthType.Google);
        var content = $"{{\"postBody\":\"id_token={idToken}&providerId={providerId}\",\"requestUri\":\"http://localhost\",\"returnSecureToken\":true}}";

        var auth = await ExecuteAuthWithPostContent(GoogleIdentityUrl, content).ConfigureAwait(false);

        await RefreshUserInfo(auth).ConfigureAwait(false);
    }

    /// <summary>
    /// Sign in with provided <paramref name="email"/> and <paramref name="password"/>.
    /// </summary>
    /// <param name="email">
    /// The email of the user.
    /// </param>
    /// <param name="password">
    /// The password of the user.
    /// </param>
    /// <param name="tenantId">
    /// The account tenant id of the user.
    /// </param>
    /// <returns>
    /// The <see cref="Task"/> proxy of the specified task.
    /// </returns>
    /// <exception cref="AuthEmailNotFoundException">
    /// There is no user record corresponding to this identifier. The user may have been deleted.
    /// </exception>
    /// <exception cref="AuthInvalidPasswordException">
    /// The password is invalid or the user does not have a password.
    /// </exception>
    /// <exception cref="AuthUserDisabledException">
    /// The user account has been disabled by an administrator.
    /// </exception>
    /// <exception cref="AuthAPIKeyNotValidException">
    /// API key not valid. Please pass a valid API key.
    /// </exception>
    /// <exception cref="AuthInvalidIDTokenException">
    /// The user's credential is no longer valid. The user must sign in again.
    /// </exception>
    /// <exception cref="AuthUserNotFoundException">
    /// There is no user record corresponding to this identifier. The user may have been deleted.
    /// </exception>
    /// <exception cref="AuthUndefinedException">
    /// The error occured is undefined.
    /// </exception>
    /// <exception cref="OperationCanceledException">
    /// The operation was cancelled.
    /// </exception>
    public async Task SignInWithEmailAndPassword(string email, string password, string? tenantId = null)
    {
        StringBuilder sb = new($"{{\"email\":\"{email}\",\"password\":\"{password}\",");

        if (tenantId != null)
        {
            sb.Append($"\"tenantId\":\"{tenantId}\",");
        }

        sb.Append("\"returnSecureToken\":true}");

        var auth = await ExecuteAuthWithPostContent(GooglePasswordUrl, sb.ToString()).ConfigureAwait(false);

        await RefreshUserInfo(auth).ConfigureAwait(false);

        InvokeAuthenticationEvents();
    }

    /// <summary>
    /// Sign in a phone number with the provided <paramref name="sessionInfo"/> and <paramref name="code"/> from reCaptcha validation and sms OTP message.
    /// </summary>
    /// <param name="sessionInfo">
    /// The session info token returned from <see cref="SendVerificationCode(string, string)"/>.
    /// </param>
    /// <param name="code">
    /// The phone sms OTP code.
    /// </param>
    /// <returns>
    /// The <see cref="Task"/> proxy of the specified task.
    /// </returns>
    /// <exception cref="AuthInvalidCustomTokenException">
    /// The custom token format is incorrect or the token is invalid for some reason (e.g. expired, invalid signature etc.)
    /// </exception>
    /// <exception cref="AuthCredentialMismatchException">
    /// The custom token corresponds to a different Firebase project.
    /// </exception>
    /// <exception cref="AuthAPIKeyNotValidException">
    /// API key not valid. Please pass a valid API key.
    /// </exception>
    /// <exception cref="AuthUndefinedException">
    /// The error occured is undefined.
    /// </exception>
    /// <exception cref="OperationCanceledException">
    /// The operation was cancelled.
    /// </exception>
    public async Task SignInWithPhoneNumber(string sessionInfo, string code)
    {
        string content = $"{{\"sessionInfo\":\"{sessionInfo}\",\"code\":\"{code}\",\"returnSecureToken\":true}}";

        var auth = await ExecuteAuthWithPostContent(GoogleSignInWithPhoneNumber, content).ConfigureAwait(false);

        await RefreshUserInfo(auth).ConfigureAwait(false);

        InvokeAuthenticationEvents();
    }

    /// <summary>
    /// Sign in anonimously.
    /// </summary>
    /// <returns>
    /// The <see cref="Task"/> proxy of the specified task.
    /// </returns>
    /// <exception cref="AuthOperationNotAllowedException">
    /// Anonymous user sign-in is disabled for this project.
    /// </exception>
    /// <exception cref="AuthAPIKeyNotValidException">
    /// API key not valid. Please pass a valid API key.
    /// </exception>
    /// <exception cref="AuthInvalidIDTokenException">
    /// The user's credential is no longer valid. The user must sign in again.
    /// </exception>
    /// <exception cref="AuthUserNotFoundException">
    /// There is no user record corresponding to this identifier. The user may have been deleted.
    /// </exception>
    /// <exception cref="AuthUndefinedException">
    /// The error occured is undefined.
    /// </exception>
    /// <exception cref="OperationCanceledException">
    /// The operation was cancelled.
    /// </exception>
    public async Task SignInAnonymously()
    {
        var content = $"{{\"returnSecureToken\":true}}";

        var auth = await ExecuteAuthWithPostContent(GoogleSignUpUrl, content).ConfigureAwait(false);

        await RefreshUserInfo(auth).ConfigureAwait(false);

        InvokeAuthenticationEvents();
    }

    /// <summary>
    /// Send password reset email to the existing account provided with the <paramref name="email"/>.
    /// </summary>
    /// <param name="email">
    /// The email of the user to send the password reset.
    /// </param>
    /// <returns>
    /// The <see cref="Task"/> proxy of the specified task.
    /// </returns>
    /// <exception cref="AuthEmailNotFoundException">
    /// There is no user record corresponding to this identifier. The user may have been deleted.
    /// </exception>
    /// <exception cref="AuthAPIKeyNotValidException">
    /// API key not valid. Please pass a valid API key.
    /// </exception>
    /// <exception cref="AuthUndefinedException">
    /// The error occured is undefined.
    /// </exception>
    /// <exception cref="OperationCanceledException">
    /// The operation was cancelled.
    /// </exception>
    public async Task SendPasswordResetEmail(string email)
    {
        var responseData = "N/A";

        try
        {
            var content = $"{{\"requestType\":\"PASSWORD_RESET\",\"email\":\"{email}\"}}";
            var response = await GetClient().PostAsync(
                new Uri(string.Format(GoogleGetConfirmationCodeUrl, App.Config.ApiKey)),
                new StringContent(content, Encoding.UTF8, "Application/json"),
                new CancellationTokenSource(App.Config.AuthRequestTimeout).Token).ConfigureAwait(false);
            responseData = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            response.EnsureSuccessStatusCode();
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw ExceptionHelpers.GetException(responseData, ex);
        }
    }

    /// <summary>
    /// Copies authentication from another <see cref="RestfulFirebaseApp"/>.
    /// </summary>
    /// <param name="app">
    /// The <see cref="RestfulFirebaseApp"/> to copy from
    /// </param>
    public void CopyAuthenticationFrom(RestfulFirebaseApp app)
    {
        session.CopyAuthenticationFrom(app);
    }

    internal void OnAuthRefreshed()
    {
        AuthRefreshed?.Invoke(this, new EventArgs());
    }

    internal void InvokeAuthenticationEvents()
    {
        if (IsAuthenticated)
        {
            Authenticated?.Invoke(this, new EventArgs());
        }
        else
        {
            Unauthenticated?.Invoke(this, new EventArgs());
        }
        AuthenticationChanges?.Invoke(this, new AuthenticationChangesEventArgs(IsAuthenticated));
    }

    #endregion

    #region Disposable Members

    /// <summary>
    /// The dispose logic.
    /// </summary>
    /// <param name = "disposing">
    /// Whether the method is being called in response to disposal, or finalization.
    /// </param>
    protected void Dispose(bool disposing)
    {
        if (disposing)
        {
            client?.Dispose();
        }
    }

    #endregion
}
