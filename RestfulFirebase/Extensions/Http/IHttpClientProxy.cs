using System;
using System.Net.Http;

namespace RestfulFirebase.Extensions.Http
{
    /// <summary>
    /// The <see cref="HttpClient"/> factory for custom factory declarations.
    /// </summary>
    public interface IHttpClientProxy : IDisposable
    {
        /// <summary>
        /// The factory to create <see cref="HttpClient"/> for every firebase requests.
        /// </summary>
        /// <returns>
        /// The created <see cref="HttpClient"/> for every firebase requests.
        /// </returns>
        HttpClient GetHttpClient();
    }
}
