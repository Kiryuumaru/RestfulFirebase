using System;

namespace RestfulFirebase.Http
{
    /// <summary>
    /// The <see cref="IHttpClientProxy"/> factory for custom factory declarations.
    /// </summary>
    public interface IHttpClientFactory
    {
        /// <summary>
        /// The factory used to create <see cref="IHttpClientProxy"/> for every firebase requests.
        /// </summary>
        /// <param name="timeout">
        /// The specified timeout for the client.
        /// </param>
        /// <returns>
        /// The created <see cref="IHttpClientProxy"/> for every firebase requests.
        /// </returns>
        IHttpClientProxy GetHttpClient(TimeSpan? timeout);
    }
}
