using RestfulFirebase.Authentication.Internals;
using RestfulFirebase.Authentication.Models;
using RestfulFirebase.Common.Transactions;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RestfulFirebase.Authentication.Transactions;

/// <summary>
/// Request to creates user with the provided email and password.
/// </summary>
public class CreateUserWithEmailAndPasswordRequest : AuthenticationRequest<CreateUserWithEmailAndPasswordResponse>
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
    /// Gets or sets <c>true</c> to send email verification after user creation; otherwise, <c>false</c>.
    /// </summary>
    public bool SendVerificationEmail { get; set; } = false;

    /// <inheritdoc cref="CreateUserWithEmailAndPasswordRequest"/>
    /// <returns>
    /// The <see cref="Task"/> proxy that represents the <see cref="CreateUserWithEmailAndPasswordResponse"/> with the authenticated <see cref="FirebaseUser"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <see cref="TransactionRequest.Config"/>,
    /// <see cref="Email"/> or
    /// <see cref="Password"/> is a null reference.
    /// </exception>
    internal override async Task<CreateUserWithEmailAndPasswordResponse> Execute()
    {
        ArgumentNullException.ThrowIfNull(Config);
        ArgumentNullException.ThrowIfNull(Email);
        ArgumentNullException.ThrowIfNull(Password);

        try
        {
            var content = $"{{\"email\":\"{Email}\",\"password\":\"{Password}\",\"returnSecureToken\":true}}";

            FirebaseAuth auth = await ExecuteAuthWithPostContent(content, GoogleSignUpUrl, CamelCaseJsonSerializerOption);

            FirebaseUser user = new(auth);

            await RefreshUserInfo(user);

            if (SendVerificationEmail)
            {
                await Api.Authentication.SendEmailVerification(new SendEmailVerificationRequest()
                {
                    Config = Config,
                    FirebaseUser = user,
                    CancellationToken = CancellationToken,
                    HttpClient = HttpClient
                });
            }

            return new CreateUserWithEmailAndPasswordResponse(this, user, null);
        }
        catch (Exception ex)
        {
            return new CreateUserWithEmailAndPasswordResponse(this, null, ex);
        }
    }
}

/// <summary>
/// The response of the <see cref="GetRecaptchaSiteKeyRequest"/> request.
/// </summary>
public class CreateUserWithEmailAndPasswordResponse : TransactionResponse<CreateUserWithEmailAndPasswordRequest, FirebaseUser>
{
    internal CreateUserWithEmailAndPasswordResponse(CreateUserWithEmailAndPasswordRequest request, FirebaseUser? result, Exception? error)
        : base(request, result, error)
    {

    }
}