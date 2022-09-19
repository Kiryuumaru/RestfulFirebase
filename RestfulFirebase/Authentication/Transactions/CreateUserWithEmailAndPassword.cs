using RestfulFirebase.Common.Internals;
using RestfulFirebase.Common.Models;
using System;
using System.Threading.Tasks;

namespace RestfulFirebase.Common.Transactions;

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

            return new(this, user, null);
        }
        catch (Exception ex)
        {
            return new(this, null, ex);
        }
    }
}