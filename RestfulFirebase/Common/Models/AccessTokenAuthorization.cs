using RestfulFirebase.Common.Abstractions;

namespace RestfulFirebase.Common.Models;

/// <summary>
/// The authorization using google cloud platform token for all firebase requests.
/// </summary>
public class AccessTokenAuthorization : IAuthorization
{
    /// <inheritdoc/>
    public string Token { get; }

    /// <inheritdoc/>
    public bool IsAccessToken => true;

    /// <summary>
    /// Creates an instance of <see cref="AccessTokenAuthorization"/>.
    /// </summary>
    /// <param name="token">
    /// The access token from google cloud platform.
    /// </param>
    public AccessTokenAuthorization(string token)
    {
        Token = token;
    }
}
