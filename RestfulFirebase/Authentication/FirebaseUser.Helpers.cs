using RestfulFirebase.Common.Utilities;
using System;
using RestfulFirebase.Authentication.Internals;
using RestfulFirebase.Common.Abstractions;
using ObservableHelpers.ComponentModel;
using ObservableHelpers.ComponentModel.Enums;
using System.Threading.Tasks;
using System.Threading;
using RestfulFirebase.Common.Http;
using System.Net.Http;
using System.Text.Json;
using RestfulFirebase.Authentication.Exceptions;
using RestfulFirebase.Authentication.Enums;
using System.Linq;
using System.Data;
using System.IO;
using System.Diagnostics.CodeAnalysis;
using static RestfulFirebase.Authentication.AuthenticationApi;

namespace RestfulFirebase.Authentication;

/// <summary>
/// Provides firebase user authentication implementations.
/// </summary>
public partial class FirebaseUser
{
#if NET5_0_OR_GREATER
    [DynamicDependency(DynamicallyAccessedMemberTypes.PublicProperties, typeof(FirebaseAuth))]
    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
#endif
    internal async Task<HttpResponse> RefreshUserInfo(CancellationToken cancellationToken)
    {
        HttpResponse response = new();

        var tokenResponse = await GetFreshToken(cancellationToken);
        response.Append(tokenResponse);
        if (tokenResponse.IsError)
        {
            return response;
        }

        using MemoryStream stream = new();
        Utf8JsonWriter writer = new(stream);

        writer.WriteStartObject();
        writer.WritePropertyName("idToken");
        writer.WriteStringValue(tokenResponse.Result);
        writer.WriteEndObject();

        await writer.FlushAsync(cancellationToken);
        
        var postResponse = await App.Authentication.ExecutePost(stream, GoogleGetUser, cancellationToken);
        response.Append(postResponse);
        if (postResponse.IsError || postResponse.HttpTransactions.LastOrDefault() is not HttpTransaction lastHttpTransaction)
        {
            return response;
        }

#if NET6_0_OR_GREATER
        var responseData = await lastHttpTransaction.HttpResponseMessage.Content.ReadAsStreamAsync(cancellationToken);
#else
        var responseData = await lastHttpTransaction.HttpResponseMessage.Content.ReadAsStreamAsync();
#endif

        JsonDocument resultJson = JsonDocument.Parse(responseData);
        if (!resultJson.RootElement.TryGetProperty("users", out JsonElement userJson))
        {
            return new(lastHttpTransaction.HttpRequestMessage, lastHttpTransaction.HttpResponseMessage, lastHttpTransaction.HttpStatusCode,
                new FirebaseAuthenticationException(AuthErrorType.UndefinedException, "Unknown error occured.", default, default, default, default, default));
        }

        var auth = JsonSerializer.Deserialize<FirebaseAuth>(userJson.EnumerateArray().First(), JsonSerializerHelpers.CamelCaseJsonSerializerOption);

        if (auth == null)
        {
            return new(lastHttpTransaction.HttpRequestMessage, lastHttpTransaction.HttpResponseMessage, lastHttpTransaction.HttpStatusCode,
                new FirebaseAuthenticationException(AuthErrorType.UndefinedException, "Unknown error occured.", default, default, default, default, default));
        }

        UpdateAuth(auth);
        UpdateInfo(auth);

        return response;
    }

#if NET5_0_OR_GREATER
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(FirebaseAuth))]
    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
#endif
    private async Task<HttpResponse> ExecuteUser(MemoryStream stream, string googleUrl, CancellationToken cancellationToken)
    {
        HttpResponse response = new();

        var postResponse = await App.Authentication.ExecutePost(stream, googleUrl, cancellationToken);
        response.Append(postResponse);
        if (postResponse.IsError)
        {
            return response;
        }

        var responseRefresh = await RefreshUserInfo(cancellationToken);
        response.Append(responseRefresh);
        if (responseRefresh.IsError)
        {
            return response;
        }

        return response;
    }

    internal void UpdateAuth(FirebaseAuth auth)
    {
        if (auth.IdToken != null && auth.IdToken != idToken)
        {
            idToken = auth.IdToken;
            Created = DateTimeOffset.UtcNow;
        }
        if (auth.RefreshToken != null && auth.RefreshToken != RefreshToken)
        {
            RefreshToken = auth.RefreshToken;
        }
        if (auth.ExpiresIn.HasValue && auth.ExpiresIn.Value != ExpiresIn)
        {
            ExpiresIn = auth.ExpiresIn.Value;
        }
        if (auth.LocalId != null && auth.LocalId != LocalId)
        {
            LocalId = auth.LocalId;
        }
    }

    internal void UpdateInfo(FirebaseAuth auth)
    {
        FederatedId = auth.FederatedId;
        FirstName = auth.FirstName;
        LastName = auth.LastName;
        DisplayName = auth.DisplayName;
        Email = auth.Email;
        IsEmailVerified = auth.IsEmailVerified;
        PhoneNumber = auth.PhoneNumber;
    }
}
