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

        var (executeResult, executeException) = await ExecuteWithGet<RecaptchaSiteKeyDefinition>(GoogleRecaptchaParams, CamelCaseJsonSerializerOption);
        if (executeResult?.RecaptchaSiteKey == null)
        {
            return new(this, null, executeException);
        }

        return new(this, executeResult.RecaptchaSiteKey, null);
    }
}
