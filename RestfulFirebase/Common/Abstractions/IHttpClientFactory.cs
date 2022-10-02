using System.Net.Http;

namespace RestfulFirebase.Http;

/// <summary>
/// The <see cref="HttpClient"/> factory for custom factory declarations.
/// </summary>
public interface IHttpClientFactory
{
    /// <summary>
    /// The factory used to create <see cref="HttpClient"/> for every firebase requests.
    /// </summary>
    /// <returns>
    /// The created <see cref="HttpClient"/> for every firebase requests.
    /// </returns>
    HttpClient GetHttpClient();
}
