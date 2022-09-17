﻿using RestfulFirebase.Authentication.Enums;
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
/// Request to sign in with oauth provided with auth type and oauth token.
/// </summary>
public class SignInWithOAuthRequest : AuthenticationRequest<SignInWithOAuthResponse>
{
    /// <summary>
    /// Gets or sets the <see cref="FirebaseAuthType"/> of the oauth used.
    /// </summary>
    public FirebaseAuthType? AuthType { get; set; }

    /// <summary>
    /// Gets or sets the token of the provided oauth type.
    /// </summary>
    public string? OAuthToken { get; set; }

    /// <inheritdoc cref="SignInWithOAuthRequest"/>
    /// <returns>
    /// The <see cref="Task"/> proxy that represents the <see cref="SignInWithOAuthResponse"/> with the authenticated <see cref="FirebaseUser"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <see cref="TransactionRequest.Config"/>,
    /// <see cref="AuthType"/> or
    /// <see cref="OAuthToken"/> is a null reference.
    /// </exception>
    internal override async Task<SignInWithOAuthResponse> Execute()
    {
        ArgumentNullException.ThrowIfNull(Config);
        ArgumentNullException.ThrowIfNull(AuthType);
        ArgumentNullException.ThrowIfNull(OAuthToken);

        try
        {
            var providerId = GetProviderId(AuthType.Value);

            string content = AuthType.Value switch
            {
                FirebaseAuthType.Apple => $"{{\"postBody\":\"id_token={OAuthToken}&providerId={providerId}\",\"requestUri\":\"http://localhost\",\"returnSecureToken\":true}}",
                _ => $"{{\"postBody\":\"access_token={OAuthToken}&providerId={providerId}\",\"requestUri\":\"http://localhost\",\"returnSecureToken\":true}}",
            };

            FirebaseAuth auth = await ExecuteAuthWithPostContent(content, GoogleIdentityUrl, CamelCaseJsonSerializerOption);

            FirebaseUser user = new(auth);

            await RefreshUserInfo(user);

            return new SignInWithOAuthResponse(this, user, null);
        }
        catch (Exception ex)
        {
            return new SignInWithOAuthResponse(this, null, ex);
        }
    }
}

/// <summary>
/// The response of the <see cref="SignInWithOAuthRequest"/> 
/// </summary>
public class SignInWithOAuthResponse : TransactionResponse<SignInWithOAuthRequest, FirebaseUser>
{
    internal SignInWithOAuthResponse(SignInWithOAuthRequest request, FirebaseUser? result, Exception? error)
        : base(request, result, error)
    {

    }
}
