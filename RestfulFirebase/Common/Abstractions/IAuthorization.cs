namespace RestfulFirebase.Common.Abstractions;

/// <summary>
/// The authorization for all firebase requests.
/// </summary>
public interface IAuthorization
{
    /// <summary>
    /// Gets the token for the authorization requests.
    /// </summary>
    string Token { get; }

    /// <summary>
    /// Gets <c>true</c> if the token is an access token or a firebase user token; otherwise, <c>false</c>.
    /// </summary>
    bool IsAccessToken { get; }
}
