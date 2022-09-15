using RestfulFirebase.Common.Transactions;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RestfulFirebase.Authentication.Transactions;

/// <summary>
/// Request to link the account with the provided email and password.
/// </summary>
public class LinkAccountRequest : AuthenticatedRequest
{
    /// <summary>
    /// Gets or sets the account`s email to be linked.
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Gets or sets the account`s password to be linked.
    /// </summary>
    public string? Password { get; set; }

    /// <inheritdoc cref="LinkAccountRequest"/>
    /// <returns>
    /// The <see cref="Task"/> proxy that represents the <see cref="AuthenticatedResponse"/> with the authenticated <see cref="FirebaseUser"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <see cref="TransactionRequest.Config"/>,
    /// <see cref="AuthenticatedRequest.FirebaseUser"/>,
    /// <see cref="Email"/> or
    /// <see cref="Password"/> is a null reference.
    /// </exception>
    internal override async Task<AuthenticatedResponse> Execute()
    {
        ArgumentNullException.ThrowIfNull(Config);
        ArgumentNullException.ThrowIfNull(FirebaseUser);
        ArgumentNullException.ThrowIfNull(Email);
        ArgumentNullException.ThrowIfNull(Password);

        try
        {
            var tokenRequest = await Api.Authentication.GetFreshToken(this);

            if (tokenRequest.Error != null)
            {
                throw tokenRequest.Error;
            }

            var content = $"{{\"idToken\":\"{tokenRequest.Result}\",\"email\":\"{Email}\",\"password\":\"{Password}\",\"returnSecureToken\":true}}";

            await ExecuteAuthWithPostContent(content, GoogleSetAccountUrl, CamelCaseJsonSerializerOption);

            await RefreshUserInfo(FirebaseUser);

            return new AuthenticatedResponse(this, FirebaseUser, null);
        }
        catch (Exception ex)
        {
            return new AuthenticatedResponse(this, null, ex);
        }
    }
}
