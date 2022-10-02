using RestfulFirebase.Common.Http;
using RestfulFirebase.Common.Models;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace RestfulFirebase.Common.Abstractions;

/// <summary>
/// The authorization for all firebase requests.
/// </summary>
public interface IAuthorization
{
    /// <summary>
    /// Gets <c>true</c> if the token is an access token or a firebase user token; otherwise, <c>false</c>.
    /// </summary>
    bool IsAccessToken { get; }

    /// <summary>
    /// Gets the fresh token for the authorization requests.
    /// </summary>
    /// <param name="cancellationToken">
    /// The <see cref="CancellationToken"/> that propagates notification if the operation should be canceled.
    /// </param>
    /// <returns>
    /// The <see cref="ValueTask{T}"/> with <see cref="string"/> token.
    /// </returns>
    public ValueTask<Response<string>> GetFreshToken(CancellationToken cancellationToken);
}
