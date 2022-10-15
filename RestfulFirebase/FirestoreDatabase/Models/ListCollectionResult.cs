using RestfulFirebase.Common.Http;
using RestfulFirebase.FirestoreDatabase.References;
using RestfulFirebase.Common.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RestfulFirebase.FirestoreDatabase.Models;

/// <summary>
/// The result of the <see cref="FirestoreDatabaseApi.ListCollection(int?, DocumentReference?, IAuthorization?, CancellationToken)"/> request.
/// </summary>
public class ListCollectionResult : IAsyncEnumerable<HttpResponse<ListCollectionResult>>
{
    /// <summary>
    /// Gets the first result of the list.
    /// </summary>
    public IReadOnlyList<CollectionReference> CollectionReferences { get; }

    private readonly string? nextPageToken;
    private readonly HttpResponse<ListCollectionResult> firstResponse;
    private readonly Func<string, CancellationToken, Task<HttpResponse<ListCollectionResult>>> pager;

    internal ListCollectionResult(
        IReadOnlyList<CollectionReference> collectionReferences,
        string? nextPageToken,
        HttpResponse<ListCollectionResult> firstResponse,
        Func<string, CancellationToken, Task<HttpResponse<ListCollectionResult>>> pager)
    {
        CollectionReferences = collectionReferences;
        this.nextPageToken = nextPageToken;
        this.firstResponse = firstResponse;
        this.pager = pager;
    }

    /// <inheritdoc/>
    public IAsyncEnumerator<HttpResponse<ListCollectionResult>> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
        return new AsyncEnumerator(nextPageToken, firstResponse, CancellationTokenSource.CreateLinkedTokenSource(cancellationToken));
    }

    internal class AsyncEnumerator : IAsyncEnumerator<HttpResponse<ListCollectionResult>>
    {
        public HttpResponse<ListCollectionResult> Current { get; private set; } = default!;

        private ListCollectionResult lastSuccessResult;

        private readonly string? nextPageToken;
        private readonly HttpResponse<ListCollectionResult> firstResponse;
        private readonly CancellationTokenSource cancellationTokenSource;

        public AsyncEnumerator(string? nextPageToken, HttpResponse<ListCollectionResult> firstResponse, CancellationTokenSource cancellationTokenSource)
        {
            firstResponse.ThrowIfError();
            this.nextPageToken = nextPageToken;
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
                return lastSuccessResult.CollectionReferences.Count != 0;
            }
            else
            {
                if (nextPageToken == null)
                {
                    return false;
                }
                else
                {
                    Current = await lastSuccessResult.pager.Invoke(nextPageToken, cancellationTokenSource.Token);
                    if (Current.IsSuccess)
                    {
                        lastSuccessResult = Current.Result;
                    }
                    return true;
                }
            }
        }
    }
}