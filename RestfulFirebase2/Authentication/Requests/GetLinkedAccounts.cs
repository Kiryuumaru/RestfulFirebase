﻿using RestfulFirebase.Authentication.Enums;
using RestfulFirebase.Common.Requests;
using RestfulFirebase.Common.Utilities;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace RestfulFirebase.Authentication.Requests;

/// <summary>
/// Request to get all linked accounts of the user.
/// </summary>
public class GetLinkedAccountsRequest : AuthenticationRequest<TransactionResponse<GetLinkedAccountsRequest, ProviderQueryResult>>
{
    /// <summary>
    /// Gets or sets the email of the user.
    /// </summary>
    public string? Email { get; set; }

    /// <inheritdoc cref="GetLinkedAccountsRequest"/>
    /// <returns>
    /// The <see cref="Task"/> proxy that represents the <see cref="TransactionResponse"/> with the result <see cref="ProviderQueryResult"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <see cref="TransactionRequest.Config"/> or
    /// <see cref="Email"/> is a null reference.
    /// </exception>
    internal override async Task<TransactionResponse<GetLinkedAccountsRequest, ProviderQueryResult>> Execute()
    {
        ArgumentNullException.ThrowIfNull(Config);
        ArgumentNullException.ThrowIfNull(Email);

        string content = $"{{\"identifier\":\"{Email}\", \"continueUri\": \"http://localhost\"}}";

        var (executeResult, executeException)  = await ExecuteWithPostContent<ProviderQueryResult>(content, GoogleCreateAuthUrl, CamelCaseJsonSerializerOption);
        if (executeResult == null)
        {
            return new(this, null, executeException);
        }

        executeResult.Email = Email;

        return new(this, executeResult, null);
    }
}

/// <summary>
/// More info at <see href="https://developers.google.com/identity/toolkit/web/reference/relyingparty/createAuthUri"/>.
/// </summary>
public class ProviderQueryResult
{
    internal ProviderQueryResult()
    {
        Providers = new List<FirebaseAuthType>();
    }

    /// <summary>
    /// The underlying email of the auth provider.
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Gets or sets <c>true</c> whether the user is registered; otherwise <c>false</c>.
    /// </summary>
    [JsonPropertyName("registered")]
    public bool IsRegistered { get; set; }

    /// <summary>
    /// Gets or sets <c>true</c> if the <see cref="AuthUri"/> is for user's existing provider; otherwise <c>false</c>.
    /// </summary>
    [JsonPropertyName("forExistingProvider")]
    public bool IsForExistingProvider { get; set; }

    /// <summary>
    /// The URI used by the IDP to authenticate the user.
    /// </summary>
    [JsonPropertyName("authUri")]
    public string? AuthUri { get; set; }

    /// <summary>
    /// The provider ID of the auth URI.
    /// </summary>
    [JsonPropertyName("providerId")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public FirebaseAuthType? ProviderId { get; set; }

    /// <summary>
    /// All provider ID of the auth URI.
    /// </summary>
    [JsonPropertyName("allProviders")]
    [JsonConverter(typeof(ItemConverterDecorator<JsonStringEnumConverter>))]
    public List<FirebaseAuthType> Providers { get; set; }
}