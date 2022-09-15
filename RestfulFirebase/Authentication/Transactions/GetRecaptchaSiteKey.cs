using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using RestfulFirebase.FirestoreDatabase.Query;
using RestfulFirebase.FirestoreDatabase;
using RestfulFirebase.Common.Transactions;
using System.Threading.Tasks;
using System.IO;
using System.Net.Http;
using System.Diagnostics.CodeAnalysis;
using RestfulFirebase.Common;
using RestfulFirebase.Authentication.Internals;
using RestfulFirebase.Authentication.Exceptions;

namespace RestfulFirebase.Authentication.Transactions;

/// <summary>
/// Request to get the reCaptcha site key to be used for sending verification code to a phone number.
/// </summary>
public class GetRecaptchaSiteKeyRequest : AuthenticationRequest<GetRecaptchaSiteKeyResponse>
{
    /// <inheritdoc cref="GetRecaptchaSiteKeyRequest"/>
    /// <returns>
    /// The <see cref="Task"/> proxy that represents the <see cref="GetRecaptchaSiteKeyResponse"/> with the reCaptcha site key <see cref="string"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <see cref="TransactionRequest.Config"/> is a null reference.
    /// </exception>
    internal override async Task<GetRecaptchaSiteKeyResponse> Execute()
    {
        ArgumentNullException.ThrowIfNull(Config);

        try
        {
            RecaptchaSiteKeyDefinition? response = await ExecuteWithGet<RecaptchaSiteKeyDefinition>(GoogleRecaptchaParams, CamelCaseJsonSerializerOption);

            if (response == null || response.RecaptchaSiteKey == null)
            {
                throw new AuthUndefinedException();
            }

            return new GetRecaptchaSiteKeyResponse(this, response.RecaptchaSiteKey, null);
        }
        catch (Exception ex)
        {
            return new GetRecaptchaSiteKeyResponse(this, null, ex);
        }
    }
}

/// <summary>
/// The response of the <see cref="GetRecaptchaSiteKeyRequest"/> request.
/// </summary>
public class GetRecaptchaSiteKeyResponse : TransactionResponse<GetRecaptchaSiteKeyRequest, string>
{
    internal GetRecaptchaSiteKeyResponse(GetRecaptchaSiteKeyRequest request, string? result, Exception? error)
        : base(request, result, error)
    {

    }
}
