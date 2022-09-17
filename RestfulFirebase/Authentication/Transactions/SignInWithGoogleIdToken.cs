using RestfulFirebase.Authentication.Enums;
using RestfulFirebase.Authentication.Internals;
using RestfulFirebase.Common.Transactions;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RestfulFirebase.Authentication.Models;

namespace RestfulFirebase.Authentication.Transactions;

/// <summary>
/// Request to sign in with google id token.
/// </summary>
public class SignInWithGoogleIdTokenRequest : AuthenticationRequest<TransactionResponse<SignInWithGoogleIdTokenRequest, FirebaseUser>>
{
    /// <summary>
    /// Gets or sets the id token provided by google.
    /// </summary>
    public string? IdToken { get; set; }

    /// <inheritdoc cref="SignInWithGoogleIdTokenRequest"/>
    /// <returns>
    /// The <see cref="Task"/> proxy that represents the <see cref="TransactionResponse"/> with the authenticated <see cref="FirebaseUser"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <see cref="TransactionRequest.Config"/> or
    /// <see cref="IdToken"/> is a null reference.
    /// </exception>
    internal override async Task<TransactionResponse<SignInWithGoogleIdTokenRequest, FirebaseUser>> Execute()
    {
        ArgumentNullException.ThrowIfNull(Config);
        ArgumentNullException.ThrowIfNull(IdToken);

        try
        {
            var providerId = GetProviderId(FirebaseAuthType.Google);
            var content = $"{{\"postBody\":\"id_token={IdToken}&providerId={providerId}\",\"requestUri\":\"http://localhost\",\"returnSecureToken\":true}}";

            FirebaseAuth auth = await ExecuteAuthWithPostContent(content, GoogleIdentityUrl, CamelCaseJsonSerializerOption);

            FirebaseUser user = new(auth);

            await RefreshUserInfo(user);

            return new(this, user, null);
        }
        catch (Exception ex)
        {
            return new(this, null, ex);
        }
    }
}
