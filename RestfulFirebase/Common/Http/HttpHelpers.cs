using System.IO;
using System.Net;
using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace RestfulFirebase.Common.Http;

internal static class HttpHelpers
{
    internal static async Task<HttpResponse> Execute(HttpClient httpClient, HttpMethod httpMethod, string uri, CancellationToken cancellationToken)
    {
        HttpRequestMessage request = new(httpMethod, uri);
        HttpResponseMessage? response = null;
        HttpStatusCode statusCode = HttpStatusCode.OK;

        try
        {
            if (request.Content != null)
            {
                await request.Content.LoadIntoBufferAsync();
            }

            response = await httpClient.SendAsync(request, cancellationToken);

            statusCode = response.StatusCode;

            response.EnsureSuccessStatusCode();

            return new HttpResponse(request, response, statusCode, null);
        }
        catch (Exception ex)
        {
            return new HttpResponse(request, response, statusCode, ex);
        }
    }

    [RequiresUnreferencedCode("Calls System.Text.Json.JsonSerializer.Deserialize<TValue>(String, JsonSerializerOptions)")]
    internal static async Task<HttpResponse<T>> Execute<T>(HttpClient httpClient, HttpMethod httpMethod, string uri, JsonSerializerOptions jsonSerializerOptions, CancellationToken cancellationToken)
    {
        HttpRequestMessage request = new(httpMethod, uri);
        HttpResponseMessage? response = null;
        HttpStatusCode statusCode = HttpStatusCode.OK;

        try
        {
            if (request.Content != null)
            {
                await request.Content.LoadIntoBufferAsync();
            }

            response = await httpClient.SendAsync(request, cancellationToken);

            statusCode = response.StatusCode;

            response.EnsureSuccessStatusCode();

#if NET6_0_OR_GREATER
            var responseData = await response.Content.ReadAsStringAsync(cancellationToken);
#else
            var responseData = await response.Content.ReadAsStringAsync();
#endif

            return new(request, response, JsonSerializer.Deserialize<T>(responseData, jsonSerializerOptions), statusCode, null);
        }
        catch (Exception ex)
        {
            return new(request, response, default, statusCode, ex);
        }
    }

    internal static async Task<HttpResponse> ExecuteWithContent(HttpClient httpClient, Stream contentStream, HttpMethod httpMethod, string uri, CancellationToken cancellationToken)
    {
        contentStream.Seek(0, SeekOrigin.Begin);

        StreamContent streamContent = new(contentStream);
        streamContent.Headers.ContentType = new("Application/json")
        {
            CharSet = Encoding.UTF8.WebName
        };
        HttpRequestMessage request = new(httpMethod, uri)
        {
            Content = streamContent
        };
        HttpResponseMessage? response = null;
        HttpStatusCode statusCode = HttpStatusCode.OK;

        try
        {
            if (request.Content != null)
            {
                await request.Content.LoadIntoBufferAsync();
            }

            response = await httpClient.SendAsync(request, cancellationToken);

            statusCode = response.StatusCode;

            response.EnsureSuccessStatusCode();

            return new(request, response, statusCode, null);
        }
        catch (Exception ex)
        {
            return new(request, response, statusCode, ex);
        }
    }

    [RequiresUnreferencedCode("Calls System.Text.Json.JsonSerializer.Deserialize<TValue>(String, JsonSerializerOptions)")]
    internal static async Task<HttpResponse<T>> ExecuteWithContent<T>(HttpClient httpClient, Stream contentStream, HttpMethod httpMethod, string uri, JsonSerializerOptions jsonSerializerOptions, CancellationToken cancellationToken)
    {
        contentStream.Seek(0, SeekOrigin.Begin);

        StreamContent streamContent = new(contentStream);
        streamContent.Headers.ContentType = new("Application/json")
        {
            CharSet = Encoding.UTF8.WebName
        };
        HttpRequestMessage request = new(httpMethod, uri)
        {
            Content = streamContent
        };
        HttpResponseMessage? response = null;
        HttpStatusCode statusCode = HttpStatusCode.OK;

        try
        {
            if (request.Content != null)
            {
                await request.Content.LoadIntoBufferAsync();
            }

            response = await httpClient.SendAsync(request, cancellationToken);

            statusCode = response.StatusCode;

            response.EnsureSuccessStatusCode();

#if NET6_0_OR_GREATER
            var responseData = await response.Content.ReadAsStringAsync(cancellationToken);
#else
            var responseData = await response.Content.ReadAsStringAsync();
#endif

            return new(request, response, JsonSerializer.Deserialize<T>(responseData, jsonSerializerOptions), statusCode, null);
        }
        catch (Exception ex)
        {
            return new(request, response, default, statusCode, ex);
        }
    }

    internal static async Task<HttpResponse> ExecuteWithContent(HttpClient httpClient, string content, HttpMethod httpMethod, string uri, CancellationToken cancellationToken)
    {
        HttpRequestMessage request = new(httpMethod, uri)
        {
            Content = new StringContent(content, Encoding.UTF8, "Application/json")
        };
        HttpResponseMessage? response = null;
        HttpStatusCode statusCode = HttpStatusCode.OK;

        try
        {
            if (request.Content != null)
            {
                await request.Content.LoadIntoBufferAsync();
            }

            response = await httpClient.SendAsync(request, cancellationToken);

            statusCode = response.StatusCode;

            response.EnsureSuccessStatusCode();

            return new(request, response, statusCode, null);
        }
        catch (Exception ex)
        {
            return new(request, response, statusCode, ex);
        }
    }

    [RequiresUnreferencedCode("Calls System.Text.Json.JsonSerializer.Deserialize<TValue>(String, JsonSerializerOptions)")]
    internal static async Task<HttpResponse<T>> ExecuteWithContent<T>(HttpClient httpClient, string content, HttpMethod httpMethod, string uri, JsonSerializerOptions jsonSerializerOptions, CancellationToken cancellationToken)
    {
        HttpRequestMessage request = new(httpMethod, uri)
        {
            Content = new StringContent(content, Encoding.UTF8, "Application/json")
        };
        HttpResponseMessage? response = null;
        HttpStatusCode statusCode = HttpStatusCode.OK;

        try
        {
            if (request.Content != null)
            {
                await request.Content.LoadIntoBufferAsync();
            }

            response = await httpClient.SendAsync(request, cancellationToken);

            statusCode = response.StatusCode;

            response.EnsureSuccessStatusCode();

#if NET6_0_OR_GREATER
            var responseData = await response.Content.ReadAsStringAsync(cancellationToken);
#else
            var responseData = await response.Content.ReadAsStringAsync();
#endif

            return new(request, response, JsonSerializer.Deserialize<T>(responseData, jsonSerializerOptions), statusCode, null);
        }
        catch (Exception ex)
        {
            return new(request, response, default, statusCode, ex);
        }
    }
}
