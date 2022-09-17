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
/// Request to sign in with custom token provided by firebase.
/// </summary>
public class SignInWithCustomTokenRequest : AuthenticationRequest<TransactionResponse<SignInWithCustomTokenRequest, FirebaseUser>>
{
    /// <summary>
    /// Gets or sets the token provided by firebase.
    /// </summary>
    public string? CustomToken { get; set; }

    /// <inheritdoc cref="SignInWithCustomTokenRequest"/>
    /// <returns>
    /// The <see cref="Task"/> proxy that represents the <see cref="TransactionResponse"/> with the authenticated <see cref="FirebaseUser"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <see cref="TransactionRequest.Config"/> or
    /// <see cref="CustomToken"/> is a null reference.
    /// </exception>
    internal override async Task<TransactionResponse<SignInWithCustomTokenRequest, FirebaseUser>> Execute()
    {
        ArgumentNullException.ThrowIfNull(Config);
        ArgumentNullException.ThrowIfNull(CustomToken);

        try
        {
            string content = $"{{\"token\":\"{CustomToken}\",\"returnSecureToken\":true}}";

            FirebaseAuth auth = await ExecuteAuthWithPostContent(content, GoogleCustomAuthUrl, CamelCaseJsonSerializerOption);

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
