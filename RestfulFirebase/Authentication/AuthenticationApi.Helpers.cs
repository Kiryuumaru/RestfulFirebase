using System.Threading.Tasks;
using RestfulFirebase.Authentication.Internals;
using RestfulFirebase.Common.Utilities;
using System.Text.Json;
using System.Net.Http;
using System.Diagnostics.CodeAnalysis;
using RestfulFirebase.Common.Internals;
using System;
using RestfulFirebase.Authentication.Exceptions;
using RestfulFirebase.Authentication.Enums;
using System.Threading;
using RestfulFirebase.Common.Abstractions;
using System.IO;
using System.Linq;
using RestfulHelpers.Common;
using RestfulHelpers;

namespace RestfulFirebase.Authentication;

public partial class AuthenticationApi
{
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

    internal string BuildUrl(string googleUrl)
    {
        return string.Format(googleUrl, App.Config.ApiKey);
    }

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

    [RequiresUnreferencedCode(Message.RequiresUnreferencedCodeMessage)]
    internal async Task<HttpResponse<T>> ExecuteGet<T>(string googleUrl, CancellationToken cancellationToken)
    {
        var response = await App.GetHttpClient().Execute<T>(HttpMethod.Get, BuildUrl(googleUrl), JsonSerializerHelpers.CamelCaseJsonSerializerOption, cancellationToken);
        if (response.IsError)
        {
            return new(default, response, await GetHttpException(response));
        }

        return response;
    }

    internal async Task<HttpResponse> ExecutePost(MemoryStream stream, string googleUrl, CancellationToken cancellationToken)
    {
        var response = await App.GetHttpClient().ExecuteWithContent(stream, HttpMethod.Post, BuildUrl(googleUrl), cancellationToken);
        if (response.IsError)
        {
            return new(response, await GetHttpException(response));
        }

        return response;
    }

    [RequiresUnreferencedCode(Message.RequiresUnreferencedCodeMessage)]
    internal async Task<HttpResponse<T>> ExecutePost<T>(MemoryStream stream, string googleUrl, CancellationToken cancellationToken)
    {
        var response = await App.GetHttpClient().ExecuteWithContent<T>(stream, HttpMethod.Post, BuildUrl(googleUrl), JsonSerializerHelpers.CamelCaseJsonSerializerOption, cancellationToken);
        if (response.IsError)
        {
            return new(default, response, await GetHttpException(response));
        }

        return response;
    }

    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(FirebaseAuth))]
    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    internal async Task<HttpResponse<FirebaseUser>> StartUser(MemoryStream stream, string googleUrl, CancellationToken cancellationToken)
    {
        HttpResponse<FirebaseUser> response = new();

        var postResponse = await ExecutePost<FirebaseAuth>(stream, googleUrl, cancellationToken);
        response.Append(postResponse);
        if (postResponse.IsError)
        {
            return response;
        }

        FirebaseUser user = new(App, postResponse.Result);
        response.Append(user);

        var refreshResponse = await user.RefreshUserInfo(cancellationToken);
        response.Append(refreshResponse);
        if (refreshResponse.IsError)
        {
            return response;
        }

        return response;
    }
}
