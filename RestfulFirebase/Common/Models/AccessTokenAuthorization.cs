using RestfulFirebase.Common.Abstractions;
using RestfulFirebase.Common.Http;
using System.Threading;
using System.Threading.Tasks;

namespace RestfulFirebase.Common.Models;

/// <summary>
/// The authorization using google cloud platform token for all firebase requests.
/// </summary>
public class AccessTokenAuthorization : IAuthorization
{
    /// <inheritdoc/>
    public bool IsAccessToken => true;

    private readonly string token;

    /// <summary>
    /// Creates an instance of <see cref="AccessTokenAuthorization"/>.
    /// </summary>
    /// <param name="token">
    /// The access token from google cloud platform.
    /// </param>
    public AccessTokenAuthorization(string token)
    {
        this.token = token;
    }

    /// <inheritdoc/>
    public ValueTask<Response<string>> GetFreshToken(CancellationToken cancellationToken = default)
    {
        return new ValueTask<Response<string>>(new Response<string>(token, null));
    }
}
