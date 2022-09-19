using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace RestfulFirebase.FirestoreDatabase.Models;

/// <summary>
/// Represents an async pager list.
/// </summary>
/// <typeparam name="T">
/// The type of the object to iterate.
/// </typeparam>
public class AsyncPager<T> : IAsyncEnumerable<T[]>
{
    private readonly DocumentPagerIterator iterator;

    internal AsyncPager(DocumentPagerIterator iterator)
    {
        this.iterator = iterator;
    }

    /// <inheritdoc/>
    public IAsyncEnumerator<T[]> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
        return new DocumentPagerEnumerator(iterator, cancellationToken);
    }

    internal class DocumentPagerEnumerator : IAsyncEnumerator<T[]>
    {
        public T[] Current { get; private set; } = null!;

        private DocumentPagerIterator iterator;

        private readonly CancellationTokenSource cancellationTokenSource;

        public DocumentPagerEnumerator(DocumentPagerIterator iterator, CancellationToken cancellationToken)
        {
            this.iterator = iterator;
            cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        }

        public ValueTask DisposeAsync()
        {
            cancellationTokenSource.Cancel();
            return new ValueTask();
        }

        public async ValueTask<bool> MoveNextAsync()
        {
            if (iterator.NextPage != null)
            {
                iterator = await iterator.NextPage(cancellationTokenSource.Token);
                Current = iterator.Item;
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    internal class DocumentPagerIterator
    {
        public T[] Item { get; }

        public Func<CancellationToken, ValueTask<DocumentPagerIterator>>? NextPage { get; }

        public DocumentPagerIterator(T[] item, Func<CancellationToken, ValueTask<DocumentPagerIterator>>? iterator)
        {
            Item = item;
            NextPage = iterator;
        }
    }
}
