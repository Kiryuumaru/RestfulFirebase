using RestfulFirebase.Authentication.Internals;
using RestfulFirebase.Authentication.Models;
using RestfulFirebase.Common.Requests;
using System;
using System.Net;
using System.Threading.Tasks;

namespace RestfulFirebase.Authentication.Requests;

/// <summary>
/// Request to creates user with the provided email and password.
/// </summary>
public class CreateUserWithEmailAndPasswordRequest : AuthenticationRequest<TransactionResponse<CreateUserWithEmailAndPasswordRequest, FirebaseUser>>
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
    /// The <see cref="Task"/> proxy that represents the <see cref="TransactionResponse"/> with the authenticated <see cref="FirebaseUser"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <see cref="TransactionRequest.Config"/>,
    /// <see cref="Email"/> or
    /// <see cref="Password"/> is a null reference.
    /// </exception>
    internal override async Task<TransactionResponse<CreateUserWithEmailAndPasswordRequest, FirebaseUser>> Execute()
    {
        ArgumentNullException.ThrowIfNull(Config);
        ArgumentNullException.ThrowIfNull(Email);
        ArgumentNullException.ThrowIfNull(Password);

        var content = $"{{\"email\":\"{Email}\",\"password\":\"{Password}\",\"returnSecureToken\":true}}";

        var (executeResult, executeException) = await ExecuteAuthWithPostContent(content, GoogleSignUpUrl, CamelCaseJsonSerializerOption);
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

        if (SendVerificationEmail)
        {
            var sendVerificationRequest = await Api.Authentication.SendEmailVerification(new SendEmailVerificationRequest()
            {
                Config = Config,
                Authorization = user,
                CancellationToken = CancellationToken,
                HttpClient = HttpClient
            });
            if (sendVerificationRequest.Result == null)
            {
                return new(this, null, sendVerificationRequest.Error);
            }
        }

        return new(this, user, null);
    }
}