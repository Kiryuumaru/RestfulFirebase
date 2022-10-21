using RestfulFirebase.Common.Abstractions;
using RestfulFirebase.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace RestfulFirebase.Common.Http;

/// <summary>
/// The base response for all HTTP requests.
/// </summary>
public class HttpResponse : Response, IHttpResponse
{
    /// <inheritdoc/>
    public IReadOnlyList<HttpTransaction> HttpTransactions { get; }

    private readonly List<HttpTransaction> httpTransactions;

    internal HttpResponse()
        : this(default(Exception))
    {

    }

    internal HttpResponse(IHttpResponse response)
        : this(response.Error)
    {
        httpTransactions.AddRange(response.HttpTransactions);
    }

    internal HttpResponse(IHttpResponse response, Exception? error)
        : this(error)
    {
        httpTransactions.AddRange(response.HttpTransactions);
    }

    internal HttpResponse(HttpRequestMessage request, HttpResponseMessage response, HttpStatusCode httpStatusCode, Exception? error)
        : this(error)
    {
        httpTransactions.Add(new(request, response, httpStatusCode));
    }

    internal HttpResponse(Exception? error)
        : base(error)
    {
        httpTransactions = new();
        HttpTransactions = httpTransactions.AsReadOnly();
    }

    internal HttpResponse Append(params IHttpResponse[] responses)
    {
        if (responses.LastOrDefault() is IHttpResponse lastResponse)
        {
            Error = lastResponse.Error;
        }
        foreach (var response in responses)
        {
            httpTransactions.AddRange(response.HttpTransactions);
        }
        return this;
    }

    internal HttpResponse Append(Exception? error)
    {
        Error = error;
        return this;
    }

    /// <summary>
    /// Gets the string representation of the HTTP transactions.
    /// </summary>
    /// <returns>
    /// The <see cref="Task"/> that represents the string transaction contents.
    /// </returns>
    public async Task<IEnumerable<StringHttpTransaction>> GetTransactionContentsAsString()
    {
        List<StringHttpTransaction> transactions = new();
        List<Task> tasks = new();

        for (int i = 0; i < HttpTransactions.Count; i++)
        {
            int index = i;
            transactions.Add(null!);
            tasks.Add(Task.Run(async () =>
            {
                string url = HttpTransactions[index].RequestUrl;
                string? requestContent = await HttpTransactions[index].GetRequestContentAsString();
                string? responseContent = await HttpTransactions[index].GetResponseContentAsString();
                HttpStatusCode statusCode = HttpTransactions[index].StatusCode;
                transactions[index] = new StringHttpTransaction(url, requestContent, responseContent, statusCode);
            }));
        }

        await Task.WhenAll(tasks);

        return transactions;
    }
}

/// <summary>
/// The base response for all HTTP requests.
/// </summary>
/// <inheritdoc/>
public class HttpResponse<TResult> : Response<TResult>, IHttpResponse
{
    /// <inheritdoc/>
    public IReadOnlyList<HttpTransaction> HttpTransactions { get; }

    private readonly List<HttpTransaction> httpTransactions;

    internal HttpResponse()
        : this(default(TResult), default(Exception))
    {

    }

    internal HttpResponse(TResult result)
        : this(result, default(Exception))
    {

    }

    internal HttpResponse(IHttpResponse response)
        : this(default(TResult), default(Exception))
    {
        httpTransactions.AddRange(response.HttpTransactions);
    }

    internal HttpResponse(TResult? result, IHttpResponse response)
        : this(result, response.Error)
    {
        httpTransactions.AddRange(response.HttpTransactions);
    }

    internal HttpResponse(IHttpResponse response, Exception? error)
        : this(default(TResult), error)
    {
        httpTransactions.AddRange(response.HttpTransactions);
    }

    internal HttpResponse(TResult? result, IHttpResponse response, Exception? error)
        : this(result, error)
    {
        httpTransactions.AddRange(response.HttpTransactions);
    }

    internal HttpResponse(TResult? result, HttpRequestMessage request, HttpResponseMessage response, HttpStatusCode httpStatusCode, Exception? error)
        : this(result, error)
    {
        httpTransactions.Add(new(request, response, httpStatusCode));
    }

    internal HttpResponse(TResult? result, Exception? error)
        : base(result, error)
    {
        httpTransactions = new();
        HttpTransactions = httpTransactions.AsReadOnly();
    }

    internal HttpResponse<TResult> Append(params IHttpResponse[] responses)
    {
        if (responses.LastOrDefault() is IHttpResponse lastResponse)
        {
            Error = lastResponse.Error;
            if (lastResponse is HttpResponse<TResult> lastTypedResponse)
            {
                Result = lastTypedResponse.Result;
            }
        }
        foreach (var response in responses)
        {
            httpTransactions.AddRange(response.HttpTransactions);
        }
        return this;
    }

    internal HttpResponse<TResult> Append(TResult? result)
    {
        Result = result;
        return this;
    }

    internal HttpResponse<TResult> Append(Exception? error)
    {
        Error = error;
        return this;
    }

    /// <summary>
    /// Gets the string representation of the HTTP transactions.
    /// </summary>
    /// <returns>
    /// The <see cref="Task"/> that represents the string transaction contents.
    /// </returns>
    public async Task<IEnumerable<StringHttpTransaction>> GetTransactionContentsAsString()
    {
        List<StringHttpTransaction> transactions = new();
        List<Task> tasks = new();

        for (int i = 0; i < HttpTransactions.Count; i++)
        {
            int index = i;
            transactions.Add(null!);
            tasks.Add(Task.Run(async () =>
            {
                string url = HttpTransactions[index].RequestUrl;
                string? requestContent = await HttpTransactions[index].GetRequestContentAsString();
                string? responseContent = await HttpTransactions[index].GetResponseContentAsString();
                HttpStatusCode statusCode = HttpTransactions[index].StatusCode;
                transactions[index] = new StringHttpTransaction(url, requestContent, responseContent, statusCode);
            }));
        }

        await Task.WhenAll(tasks);

        return transactions;
    }
}
