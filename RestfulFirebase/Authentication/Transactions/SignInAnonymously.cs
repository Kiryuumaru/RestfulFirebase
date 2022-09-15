using RestfulFirebase.Authentication.Enums;
using RestfulFirebase.Authentication.Internals;
using RestfulFirebase.Common.Transactions;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RestfulFirebase.Authentication.Transactions;

/// <summary>
/// Request to sign in anonimously.
/// </summary>
public class SignInAnonymouslyRequest : AuthenticationRequest<SignInAnonymouslyResponse>
{
    /// <inheritdoc cref="SignInAnonymouslyRequest"/>
    /// <returns>
    /// The <see cref="Task"/> proxy that represents the <see cref="SignInAnonymouslyResponse"/> with the authenticated <see cref="FirebaseUser"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <see cref="TransactionRequest.Config"/> is a null reference.
    /// </exception>
    internal override async Task<SignInAnonymouslyResponse> Execute()
    {
        ArgumentNullException.ThrowIfNull(Config);

        try
        {
            var content = $"{{\"returnSecureToken\":true}}";

            FirebaseAuth auth = await ExecuteAuthWithPostContent(content, GoogleSignUpUrl, CamelCaseJsonSerializerOption);

            FirebaseUser user = new(auth);

            await RefreshUserInfo(user);

            return new SignInAnonymouslyResponse(this, user, null);
        }
        catch (Exception ex)
        {
            return new SignInAnonymouslyResponse(this, null, ex);
        }
    }
}

/// <summary>
/// The response of the <see cref="SignInAnonymouslyRequest"/> 
/// </summary>
public class SignInAnonymouslyResponse : TransactionResponse<SignInAnonymouslyRequest, FirebaseUser>
{
    internal SignInAnonymouslyResponse(SignInAnonymouslyRequest request, FirebaseUser? result, Exception? error)
        : base(request, result, error)
    {

    }
}
