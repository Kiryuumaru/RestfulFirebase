using RestfulFirebase.Authentication.Internals;
using RestfulFirebase.Common.Requests;
using System.Threading.Tasks;

namespace RestfulFirebase.Authentication.Requests;

/// <summary>
/// Request to send a verification code to a phone number.
/// </summary>
public class SendVerificationCodeRequest : AuthenticationRequest<TransactionResponse<SendVerificationCodeRequest, string>>
{
    /// <summary>
    /// The phone number to send verification code.
    /// </summary>
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// The recaptcha token from Google reCaptcha.
    /// </summary>
    public string? RecaptchaToken { get; set; }

    /// <inheritdoc cref="SendVerificationCodeRequest"/>
    /// <returns>
    /// The <see cref="Task"/> proxy that represents the <see cref="TransactionResponse"/> with the session info <see cref="string"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <see cref="TransactionRequest.Config"/>,
    /// <see cref="PhoneNumber"/> or
    /// <see cref="RecaptchaToken"/> is a null reference.
    /// </exception>
    internal override async Task<TransactionResponse<SendVerificationCodeRequest, string>> Execute()
    {
        ArgumentNullException.ThrowIfNull(Config);
        ArgumentNullException.ThrowIfNull(PhoneNumber);
        ArgumentNullException.ThrowIfNull(RecaptchaToken);

        string content = $"{{\"phoneNumber\":\"{PhoneNumber}\",\"recaptchaToken\":\"{RecaptchaToken}\"}}";

        var (executeResult, executeException) = await ExecuteWithPostContent<SessionInfoDefinition>(content, GoogleSendVerificationCode, CamelCaseJsonSerializerOption);
        if (executeResult?.SessionInfo == null)
        {
            return new(this, null, executeException);
        }

        return new(this, executeResult.SessionInfo, null);
    }
}
