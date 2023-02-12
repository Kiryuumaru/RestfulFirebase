using RestfulFirebase.Common.Utilities;
using System;
using RestfulFirebase.Authentication.Internals;
using System.Threading.Tasks;
using System.Threading;
using System.Text.Json;
using RestfulFirebase.Authentication.Exceptions;
using RestfulFirebase.Authentication.Enums;
using System.Linq;
using System.IO;
using System.Diagnostics.CodeAnalysis;
using RestfulHelpers.Common;
using static RestfulFirebase.Authentication.AuthenticationApi;

namespace RestfulFirebase.Authentication;

/// <summary>
/// Provides firebase user authentication implementations.
/// </summary>
public partial class FirebaseUser
{
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(FirebaseAuth))]
    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    internal async ValueTask<HttpResponse<string>> GetFreshTokenInternal(CancellationToken cancellationToken = default)
    {
        HttpResponse<string> response = new();

        if (!IsExpired())
        {
            response.Append(idToken);

            return response;
        }

        using MemoryStream stream = new();
        Utf8JsonWriter writer = new(stream);

        writer.WriteStartObject();
        writer.WritePropertyName("grant_type");
        writer.WriteStringValue("refresh_token");
        writer.WritePropertyName("refresh_token");
        writer.WriteStringValue(RefreshToken);
        writer.WriteEndObject();

        await writer.FlushAsync(cancellationToken);

        var postResponse = await App.Authentication.ExecutePost<FirebaseAuth>(stream, GoogleRefreshAuth, JsonSerializerHelpers.SnakeCaseJsonSerializerOption, cancellationToken);
        response.Append(postResponse);
        if (postResponse.IsError)
        {
            return response;
        }

        UpdateAuth(postResponse.Result);

        response.Append(idToken);

        return response;
    }

    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(FirebaseAuth))]
    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    internal async Task<HttpResponse> RefreshUserInfo(CancellationToken cancellationToken)
    {
        HttpResponse response = new();

        var tokenResponse = await GetFreshTokenInternal(cancellationToken);
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
        var responseData = lastHttpTransaction.ResponseMessage == null ? null : await lastHttpTransaction.ResponseMessage.Content.ReadAsStreamAsync(cancellationToken);
#else
        var responseData = lastHttpTransaction.ResponseMessage == null ? null : await lastHttpTransaction.ResponseMessage.Content.ReadAsStreamAsync();
#endif
        if (responseData == null)
        {
            return new(lastHttpTransaction.RequestMessage, lastHttpTransaction.ResponseMessage, lastHttpTransaction.StatusCode,
                new FirebaseAuthenticationException(AuthErrorType.UndefinedException, "Unknown error occured.", default, default, default, default, default));
        }

        JsonDocument resultJson = JsonDocument.Parse(responseData);
        if (!resultJson.RootElement.TryGetProperty("users", out JsonElement userJson))
        {
            return new(lastHttpTransaction.RequestMessage, lastHttpTransaction.ResponseMessage, lastHttpTransaction.StatusCode,
                new FirebaseAuthenticationException(AuthErrorType.UndefinedException, "Unknown error occured.", default, default, default, default, default));
        }

        var auth = JsonSerializer.Deserialize<FirebaseAuth>(userJson.EnumerateArray().First(), JsonSerializerHelpers.CamelCaseJsonSerializerOption);

        if (auth == null)
        {
            return new(lastHttpTransaction.RequestMessage, lastHttpTransaction.ResponseMessage, lastHttpTransaction.StatusCode,
                new FirebaseAuthenticationException(AuthErrorType.UndefinedException, "Unknown error occured.", default, default, default, default, default));
        }

        UpdateAuth(auth);
        UpdateInfo(auth);

        return response;
    }

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
