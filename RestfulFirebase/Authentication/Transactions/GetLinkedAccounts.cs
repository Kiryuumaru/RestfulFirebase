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
/// Request to get all linked accounts of the user.
/// </summary>
public class GetLinkedAccountsRequest : AuthenticationRequest<GetLinkedAccountsResponse>
{
    /// <summary>
    /// Gets or sets the email of the user.
    /// </summary>
    public string? Email { get; set; }

    /// <inheritdoc cref="GetLinkedAccountsRequest"/>
    /// <returns>
    /// The <see cref="Task"/> proxy that represents the <see cref="GetLinkedAccountsResponse"/> with the reCaptcha site key <see cref="string"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <see cref="TransactionRequest.Config"/> or
    /// <see cref="Email"/> is a null reference.
    /// </exception>
    internal override async Task<GetLinkedAccountsResponse> Execute()
    {
        ArgumentNullException.ThrowIfNull(Config);
        ArgumentNullException.ThrowIfNull(Email);

        try
        {
            string content = $"{{\"identifier\":\"{Email}\", \"continueUri\": \"http://localhost\"}}";

            ProviderQueryResult? data = await ExecuteWithPostContent<ProviderQueryResult>(content, GoogleCreateAuthUrl, CamelCaseJsonSerializerOption);

            if (data == null)
            {
                throw new AuthUndefinedException();
            }

            data.Email = Email;

            return new GetLinkedAccountsResponse(this, data, null);
        }
        catch (Exception ex)
        {
            return new GetLinkedAccountsResponse(this, null, ex);
        }
    }
}

/// <summary>
/// The response of the <see cref="GetLinkedAccountsRequest"/> request.
/// </summary>
public class GetLinkedAccountsResponse : TransactionResponse<GetLinkedAccountsRequest, ProviderQueryResult>
{
    internal GetLinkedAccountsResponse(GetLinkedAccountsRequest request, ProviderQueryResult? result, Exception? error)
        : base(request, result, error)
    {

    }
}
