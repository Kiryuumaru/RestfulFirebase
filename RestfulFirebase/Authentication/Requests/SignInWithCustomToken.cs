using RestfulFirebase.Authentication.Internals;
using RestfulFirebase.Authentication.Models;
using RestfulFirebase.Common.Requests;
using System;
using System.Threading.Tasks;

namespace RestfulFirebase.Authentication.Requests;

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

        string content = $"{{\"token\":\"{CustomToken}\",\"returnSecureToken\":true}}";

        var (executeResult, executeException) = await ExecuteAuthWithPostContent(content, GoogleCustomAuthUrl, CamelCaseJsonSerializerOption);
        if (executeResult == null)
        {
            return new(this, null, executeException);
        }

        FirebaseUser user = new(executeResult);

        var refreshException = await RefreshUserInfo(user);
        if (refreshException != null)
        {
            return new(this, null, refreshException);
        }

        return new(this, user, null);
    }
}
