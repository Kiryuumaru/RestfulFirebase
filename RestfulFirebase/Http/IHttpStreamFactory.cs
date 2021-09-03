using System;
using System.Net.Http;

namespace RestfulFirebase.Http
{
    /// <summary>
    /// The Http stream factory for custom streamer factory declarations.
    /// </summary>
    public interface IHttpStreamFactory
    {
        /// <summary>
        /// The factory to create <see cref="HttpClient"/> for the streamers.
        /// </summary>
        /// <returns>
        /// The created <see cref="HttpClient"/> for the streamers.
        /// </returns>
        HttpClient GetHttpClient();

        /// <summary>
        /// The factory to create <see cref="HttpRequestMessage"/> for streaming request.
        /// </summary>
        /// <param name="method">
        /// The <see cref="HttpMethod"/> used by the stream.
        /// </param>
        /// <param name="url">
        /// The url of the database stream.
        /// </param>
        /// <returns>
        /// The created <see cref="HttpRequestMessage"/> for streaming request.
        /// </returns>
        HttpRequestMessage GetStreamHttpRequestMessage(HttpMethod method, string url);
    }
}
