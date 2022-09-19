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
    /// <see cref="AuthenticatedRequest.FirebaseUser"/> is a null reference.
    /// </exception>
    internal override async Task<TransactionResponse<AuthenticatedRequest, FirebaseUser>> Execute()
    {
        ArgumentNullException.ThrowIfNull(Config);
        ArgumentNullException.ThrowIfNull(FirebaseUser);

        try
        {
            var tokenRequest = await Api.Authentication.GetFreshToken(this);

            tokenRequest.ThrowIfErrorOrEmptyResult();

            StringBuilder sb = new($"{{\"idToken\":\"{tokenRequest.Result}\"");
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

            await ExecuteAuthWithPostContent(content, GoogleSetAccountUrl, CamelCaseJsonSerializerOption);

            await RefreshUserInfo(FirebaseUser);

            return new(this, FirebaseUser, null);
        }
        catch (Exception ex)
        {
            return new(this, null, ex);
        }
    }
}
