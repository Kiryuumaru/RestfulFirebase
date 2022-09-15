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
/// Request to sign in with provided email and password.
/// </summary>
public class SignInWithEmailAndPasswordRequest : AuthenticationRequest<SignInWithEmailAndPasswordResponse>
{
    /// <summary>
    /// Gets or sets the email of the user.
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Gets or sets the password of the user.
    /// </summary>
    public string? Password { get; set; }

    /// <summary>
    /// Gets or sets the account tenant id of the user.
    /// </summary>
    public string? TenantId { get; set; } = null;

    /// <inheritdoc cref="SignInWithEmailAndPasswordRequest"/>
    /// <returns>
    /// The <see cref="Task"/> proxy that represents the <see cref="SignInWithEmailAndPasswordResponse"/> with the authenticated <see cref="FirebaseUser"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <see cref="TransactionRequest.Config"/>,
    /// <see cref="Email"/> or
    /// <see cref="Password"/> is a null reference.
    /// </exception>
    internal override async Task<SignInWithEmailAndPasswordResponse> Execute()
    {
        ArgumentNullException.ThrowIfNull(Config);
        ArgumentNullException.ThrowIfNull(Email);
        ArgumentNullException.ThrowIfNull(Password);

        try
        {
            StringBuilder sb = new($"{{\"email\":\"{Email}\",\"password\":\"{Password}\",");

            if (TenantId != null)
            {
                sb.Append($"\"tenantId\":\"{TenantId}\",");
            }

            sb.Append("\"returnSecureToken\":true}");

            string content = sb.ToString();

            FirebaseAuth auth = await ExecuteAuthWithPostContent(content, GooglePasswordUrl, CamelCaseJsonSerializerOption);

            FirebaseUser user = new(auth);

            await RefreshUserInfo(user);

            return new SignInWithEmailAndPasswordResponse(this, user, null);
        }
        catch (Exception ex)
        {
            return new SignInWithEmailAndPasswordResponse(this, null, ex);
        }
    }
}

/// <summary>
/// The response of the <see cref="SignInWithEmailAndPasswordRequest"/> 
/// </summary>
public class SignInWithEmailAndPasswordResponse : TransactionResponse<SignInWithEmailAndPasswordRequest, FirebaseUser>
{
    internal SignInWithEmailAndPasswordResponse(SignInWithEmailAndPasswordRequest request, FirebaseUser? result, Exception? error)
        : base(request, result, error)
    {

    }
}
