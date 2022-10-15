using RestfulFirebase.Common.Http;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RestfulFirebase.Common.Abstractions;
using RestfulFirebase.FirestoreDatabase.Models;
using RestfulFirebase.FirestoreDatabase.Queries;
using RestfulFirebase.FirestoreDatabase.Transactions;

namespace RestfulFirebase.FirestoreDatabase.Models;

/// <summary>
/// The result of the <see cref="FirestoreDatabaseApi.QueryDocument{TQuery}(BaseQuery{TQuery}, IEnumerable{Document}?, Transaction?, IAuthorization?, CancellationToken)"/> request.
/// </summary>
public class QueryDocumentResult : IAsyncEnumerable<HttpResponse<QueryDocumentResult>>
{
    /// <summary>
    /// Gets the found document.
    /// </summary>
    public IReadOnlyList<DocumentTimestamp> Documents { get; internal set; }

    /// <summary>
    /// Gets the number of results that have been skipped due to an offset between the last response and the current response.
    /// </summary>
    public int? SkippedResults { get; internal set; }

    /// <summary>
    /// Gets the time at which the skipped document was read.
    /// </summary>
    public DateTimeOffset? SkippedReadTime { get; internal set; }

    /// <summary>
    /// Gets the page number of the current page.
    /// </summary>
    public int CurrentPage { get; internal set; }

    private readonly int pageSize;
    private readonly HttpResponse<QueryDocumentResult> firstResponse;
    private readonly Func<int, CancellationToken, Task<HttpResponse<QueryDocumentResult>>> pager;

    internal QueryDocumentResult(
        IReadOnlyList<DocumentTimestamp> documents,
        int? skippedResults,
        DateTimeOffset? skippedReadTime,
        int currentPage,
        int pageSize,
        HttpResponse<QueryDocumentResult> firstResponse,
        Func<int, CancellationToken, Task<HttpResponse<QueryDocumentResult>>> pager)
    {
        Documents = documents;
        SkippedResults = skippedResults;
        SkippedReadTime = skippedReadTime;
        CurrentPage = currentPage;
        this.pageSize = pageSize;
        this.firstResponse = firstResponse;
        this.pager = pager;
    }

    /// <inheritdoc/>
    public IAsyncEnumerator<HttpResponse<QueryDocumentResult>> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
        return new AsyncEnumerator(pageSize, firstResponse, CancellationTokenSource.CreateLinkedTokenSource(cancellationToken));
    }

    internal class AsyncEnumerator : IAsyncEnumerator<HttpResponse<QueryDocumentResult>>
    {
        public HttpResponse<QueryDocumentResult> Current { get; private set; } = default!;

        private QueryDocumentResult lastSuccessResult;

        private readonly int pageSize;
        private readonly HttpResponse<QueryDocumentResult> firstResponse;
        private readonly CancellationTokenSource cancellationTokenSource;

        public AsyncEnumerator(int pageSize, HttpResponse<QueryDocumentResult> firstResponse, CancellationTokenSource cancellationTokenSource)
        {
            firstResponse.ThrowIfError();
            this.pageSize = pageSize;
            this.firstResponse = firstResponse;
            lastSuccessResult = firstResponse.Result;
            this.cancellationTokenSource = cancellationTokenSource;
        }

        public ValueTask DisposeAsync()
        {
            cancellationTokenSource.Cancel();
            return new ValueTask();
        }

        public async ValueTask<bool> MoveNextAsync()
        {
            if (Current == null)
            {
                Current = firstResponse;
                return lastSuccessResult.Documents.Count != 0;
            }
            else
            {
                if (lastSuccessResult.Documents.Count < pageSize)
                {
                    return false;
                }
                else
                {
                    Current = await lastSuccessResult.pager.Invoke(lastSuccessResult.CurrentPage + 1, cancellationTokenSource.Token);
                    if (Current.IsSuccess)
                    {
                        lastSuccessResult = Current.Result;
                        return lastSuccessResult.Documents.Count != 0;
                    }
                    return true;
                }
            }
        }
    }
}

/// <summary>
/// The result of the <see cref="FirestoreDatabaseApi.QueryDocument{T, TQuery}(BaseQuery{TQuery}, IEnumerable{Document}?, Transaction?, IAuthorization?, CancellationToken)"/> request.
/// </summary>
/// <typeparam name="T">
/// The type of the model of the document.
/// </typeparam>
public class QueryDocumentResult<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T> : IAsyncEnumerable<HttpResponse<QueryDocumentResult<T>>>
    where T : class
{
    /// <summary>
    /// Gets the found document.
    /// </summary>
    public IReadOnlyList<DocumentTimestamp<T>> Documents { get; internal set; }

    /// <summary>
    /// Gets the number of results that have been skipped due to an offset between the last response and the current response.
    /// </summary>
    public int? SkippedResults { get; internal set; }

    /// <summary>
    /// Gets the time at which the skipped document was read.
    /// </summary>
    public DateTimeOffset? SkippedReadTime { get; internal set; }

    /// <summary>
    /// Gets the page number of the current page.
    /// </summary>
    public int CurrentPage { get; internal set; }

    private readonly int pageSize;
    private readonly HttpResponse<QueryDocumentResult<T>> firstResponse;
    private readonly Func<int, CancellationToken, Task<HttpResponse<QueryDocumentResult<T>>>> pager;

    internal QueryDocumentResult(
        IReadOnlyList<DocumentTimestamp<T>> documents,
        int? skippedResults,
        DateTimeOffset? skippedReadTime,
        int currentPage,
        int pageSize,
        HttpResponse<QueryDocumentResult<T>> firstResponse,
        Func<int, CancellationToken, Task<HttpResponse<QueryDocumentResult<T>>>> pager)
    {
        Documents = documents;
        SkippedResults = skippedResults;
        SkippedReadTime = skippedReadTime;
        CurrentPage = currentPage;
        this.pageSize = pageSize;
        this.firstResponse = firstResponse;
        this.pager = pager;
    }

    /// <inheritdoc/>
    public IAsyncEnumerator<HttpResponse<QueryDocumentResult<T>>> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
        return new AsyncEnumerator(pageSize, firstResponse, CancellationTokenSource.CreateLinkedTokenSource(cancellationToken));
    }

    internal class AsyncEnumerator : IAsyncEnumerator<HttpResponse<QueryDocumentResult<T>>>
    {
        public HttpResponse<QueryDocumentResult<T>> Current { get; private set; } = default!;

        private QueryDocumentResult<T> lastSuccessResult;

        private readonly int pageSize;
        private readonly HttpResponse<QueryDocumentResult<T>> firstResponse;
        private readonly CancellationTokenSource cancellationTokenSource;

        public AsyncEnumerator(int pageSize, HttpResponse<QueryDocumentResult<T>> firstResponse, CancellationTokenSource cancellationTokenSource)
        {
            firstResponse.ThrowIfError();
            this.pageSize = pageSize;
            this.firstResponse = firstResponse;
            lastSuccessResult = firstResponse.Result;
            this.cancellationTokenSource = cancellationTokenSource;
        }

        public ValueTask DisposeAsync()
        {
            cancellationTokenSource.Cancel();
            return new ValueTask();
        }

        public async ValueTask<bool> MoveNextAsync()
        {
            if (Current == null)
            {
                Current = firstResponse;
                return lastSuccessResult.Documents.Count != 0;
            }
            else
            {
                if (lastSuccessResult.Documents.Count < pageSize)
                {
                    return false;
                }
                else
                {
                    Current = await lastSuccessResult.pager.Invoke(lastSuccessResult.CurrentPage + 1, cancellationTokenSource.Token);
                    if (Current.IsSuccess)
                    {
                        lastSuccessResult = Current.Result;
                        return lastSuccessResult.Documents.Count != 0;
                    }
                    return true;
                }
            }
        }
    }
}
