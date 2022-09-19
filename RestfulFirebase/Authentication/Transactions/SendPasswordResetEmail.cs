using System;
using System.Threading.Tasks;

namespace RestfulFirebase.Common.Transactions;

/// <summary>
/// Request to send password reset email to the existing account provided with the email.
/// </summary>
public class SendPasswordResetEmailRequest : AuthenticationRequest<TransactionResponse<SendPasswordResetEmailRequest>>
{
    /// <summary>
    /// Gets or sets the email of the request.
    /// </summary>
    public string? Email { get; set; }

    /// <inheritdoc cref="SendPasswordResetEmailRequest"/>
    /// <returns>
    /// The <see cref="Task"/> proxy that represents the <see cref="TransactionResponse"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <see cref="TransactionRequest.Config"/>  or
    /// <see cref="Email"/> is a null reference.
    /// </exception>
    internal override async Task<TransactionResponse<SendPasswordResetEmailRequest>> Execute()
    {
        ArgumentNullException.ThrowIfNull(Config);
        ArgumentNullException.ThrowIfNull(Email);

        try
        {
            var content = $"{{\"requestType\":\"PASSWORD_RESET\",\"email\":\"{Email}\"}}";

            await ExecuteWithPostContent(content, GoogleGetConfirmationCodeUrl);

            return new(this, null);
        }
        catch (Exception ex)
        {
            return new(this, ex);
        }
    }
}
