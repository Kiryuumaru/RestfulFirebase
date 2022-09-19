using RestfulFirebase.Authentication.Enums;
using RestfulFirebase.Authentication.Exceptions;
using RestfulFirebase.Authentication.Internals;
using RestfulFirebase.Common.Requests;
using System;
using System.Threading.Tasks;

namespace RestfulFirebase.Authentication.Requests;

/// <summary>
/// Request to get the reCaptcha site key to be used for sending verification code to a phone number.
/// </summary>
public class GetRecaptchaSiteKeyRequest : AuthenticationRequest<TransactionResponse<GetRecaptchaSiteKeyRequest, string>>
{
    /// <inheritdoc cref="GetRecaptchaSiteKeyRequest"/>
    /// <returns>
    /// The <see cref="Task"/> proxy that represents the <see cref="TransactionResponse"/> with the reCaptcha site key <see cref="string"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <see cref="TransactionRequest.Config"/> is a null reference.
    /// </exception>
    internal override async Task<TransactionResponse<GetRecaptchaSiteKeyRequest, string>> Execute()
    {
        ArgumentNullException.ThrowIfNull(Config);

        try
        {
            RecaptchaSiteKeyDefinition? response = await ExecuteWithGet<RecaptchaSiteKeyDefinition>(GoogleRecaptchaParams, CamelCaseJsonSerializerOption);

            if (response == null || response.RecaptchaSiteKey == null)
            {
                throw new FirebaseAuthenticationException(AuthErrorType.UndefinedException, "Unknown error occured.", default, default, default, default, default);
            }

            return new(this, response.RecaptchaSiteKey, null);
        }
        catch (Exception ex)
        {
            return new(this, null, ex);
        }
    }
}
