using System.Net.Http;
using System.Threading;

namespace RestfulFirebase.Common.Abstractions;

/// <summary>
/// The base request for all firebase request.
/// </summary>
public interface IRequest
{
    /// <summary>
    /// Gets or sets the config of the request.
    /// </summary>
    public FirebaseConfig? Config { get; set; }

    /// <summary>
    /// Gets or sets the <see cref="System.Net.Http.HttpClient"/> used for the request.
    /// </summary>
    public HttpClient? HttpClient { get; set; }

    /// <summary>
    /// Gets or sets the <see cref="System.Threading.CancellationToken"/> of the request.
    /// </summary>
    public CancellationToken CancellationToken { get; set; }
}
