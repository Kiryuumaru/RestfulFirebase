using RestfulFirebase.Authentication.Models;
using RestfulFirebase.Common.Requests;
using System;
using System.Text;
using System.Threading.Tasks;

namespace RestfulFirebase.Authentication.Requests;

/// <summary>
/// Request to update the accounts profile provided with display name and photo URL.
/// </summary>
public class UpdateProfileRequest : AuthenticatedRequest
{
    /// <summary>
    /// Gets or sets the new display name of the account.
    /// </summary>
    public string? DisplayName { get; set; }

    /// <summary>
    /// Gets or sets the new photo url of the account.
    /// </summary>
    public string? PhotoUrl { get; set; }

    /// <inheritdoc cref="UpdateProfileRequest"/>
    /// <returns>
    /// The <see cref="Task"/> proxy that represents the <see cref="TransactionResponse"/> with the authenticated <see cref="FirebaseUser"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <see cref="TransactionRequest.Config"/> or
    /// <see cref="AuthenticatedRequest.Authorization"/> is a null reference.
    /// </exception>
    internal override async Task<TransactionResponse<AuthenticatedRequest, FirebaseUser>> Execute()
    {
        ArgumentNullException.ThrowIfNull(Config);
        ArgumentNullException.ThrowIfNull(Authorization);

        var tokenResponse = await Api.Authentication.GetFreshToken(this);
        if (tokenResponse.Result == null)
        {
            return new(this, null, tokenResponse.Error);
        }

        StringBuilder sb = new($"{{\"idToken\":\"{tokenResponse.Result.IdToken}\"");
        if (!string.IsNullOrWhiteSpace(DisplayName) && !string.IsNullOrWhiteSpace(PhotoUrl))
        {
            sb.Append($",\"displayName\":\"{DisplayName}\",\"photoUrl\":\"{PhotoUrl}\"");
        }
        else if (!string.IsNullOrWhiteSpace(DisplayName))
        {
            sb.Append($",\"displayName\":\"{DisplayName}\"");
            sb.Append($",\"deleteAttribute\":[\"{ProfileDeletePhotoUrl}\"]");
        }
        else if (!string.IsNullOrWhiteSpace(PhotoUrl))
        {
            sb.Append($",\"photoUrl\":\"{PhotoUrl}\"");
            sb.Append($",\"deleteAttribute\":[\"{ProfileDeleteDisplayName}\"]");
        }
        else
        {
            sb.Append($",\"deleteAttribute\":[\"{ProfileDeleteDisplayName}\",\"{ProfileDeletePhotoUrl}\"]");
        }

        sb.Append($",\"returnSecureToken\":true}}");

        string content = sb.ToString();

        var (executeResult, executeException) = await ExecuteAuthWithPostContent(content, GoogleSetAccountUrl, CamelCaseJsonSerializerOption);
        if (executeResult == null)
        {
            return new(this, null, executeException);
        }

        var refreshException = await RefreshUserInfo(Authorization);
        if (refreshException != null)
        {
            return new(this, null, refreshException);
        }

        return new(this, Authorization, null);
    }
}
