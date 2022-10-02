using System.Text.Json;
using RestfulFirebase.Common.Utilities;
using System.Text.Json.Serialization;

namespace RestfulFirebase.Api;

/// <summary>
/// Provides firebase authentication implementations.
/// </summary>
public static partial class Authentication
{
    internal static readonly JsonSerializerOptions SnakeCaseJsonSerializerOption = new()
    {
        PropertyNamingPolicy = new JsonSnakeCaseNamingPolicy(),
        PropertyNameCaseInsensitive = true,
        IgnoreReadOnlyFields = true,
        NumberHandling = JsonNumberHandling.AllowReadingFromString
    };

    internal static readonly JsonSerializerOptions CamelCaseJsonSerializerOption = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        IgnoreReadOnlyFields = true,
        NumberHandling = JsonNumberHandling.AllowReadingFromString
    };

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
    internal const string GoogleUpdateUser = "https://identitytoolkit.googleapis.com/v1/accounts:update?key={0}";
    internal const string ProfileDeleteDisplayName = "DISPLAY_NAME";
    internal const string ProfileDeletePhotoUrl = "PHOTO_URL";
}
