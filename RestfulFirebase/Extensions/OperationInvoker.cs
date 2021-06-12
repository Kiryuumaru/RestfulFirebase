using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RestfulFirebase.Extensions
{
    public class OperationInvoker
    {
        public int ConcurrentTokenCount
        {
            get => tokenCount;
            set
            {
                tokenCount = value;
                EvaluateTokenCount();
            }
        }

        private SemaphoreSlim semaphoreSlim;
        private SemaphoreSlim semaphoreSlimControl;
        private int lastTokenCount;
        private int tokenCount;

        public OperationInvoker(int initialTokenCount)
        {
            semaphoreSlim = new SemaphoreSlim(initialTokenCount);
            semaphoreSlimControl = new SemaphoreSlim(1, 1);
            ConcurrentTokenCount = initialTokenCount;
        }

        private async void EvaluateTokenCount()
        {
            if (lastTokenCount == ConcurrentTokenCount) return;
            await semaphoreSlimControl.WaitAsync();
            try
            {
                if (lastTokenCount < ConcurrentTokenCount)
                {
                    int tokenToRelease = ConcurrentTokenCount - lastTokenCount;
                    lastTokenCount = ConcurrentTokenCount;
                    semaphoreSlim.Release(tokenToRelease);
                }
                else if (lastTokenCount > ConcurrentTokenCount)
                {
                    int tokenToWait = lastTokenCount - ConcurrentTokenCount;
                    lastTokenCount = ConcurrentTokenCount;
                    for (int i = 0; i < tokenToWait; i++)
                    {
                        await semaphoreSlim.WaitAsync();
                    }
                }
            }
            catch { }
            semaphoreSlimControl.Release();
        }

        public async void Post(Action action, CancellationToken? cancellationToken = null)
        {
            if (cancellationToken == null)
            {
                await semaphoreSlim.WaitAsync().ConfigureAwait(false);
                action();
            }
            else
            {
                await semaphoreSlim.WaitAsync(cancellationToken.Value).ConfigureAwait(false);
                if (!cancellationToken.Value.IsCancellationRequested) action();
            }
            semaphoreSlim.Release();
        }

        public async void Post(Func<Task> func, CancellationToken? cancellationToken = null)
        {
            if (cancellationToken == null)
            {
                await semaphoreSlim.WaitAsync().ConfigureAwait(false);
                await func().ConfigureAwait(false);
            }
            else
            {
                await semaphoreSlim.WaitAsync(cancellationToken.Value).ConfigureAwait(false);
                if (!cancellationToken.Value.IsCancellationRequested) await func().ConfigureAwait(false);
            }
            semaphoreSlim.Release();
        }

        public void Send(Action action, CancellationToken? cancellationToken = null)
        {
            if (cancellationToken == null)
            {
                semaphoreSlim.Wait();
                action();
            }
            else
            {
                semaphoreSlim.Wait(cancellationToken.Value);
                if (!cancellationToken.Value.IsCancellationRequested) action();
            }
            semaphoreSlim.Release();
        }

        public async void Send(Func<Task> func, CancellationToken? cancellationToken = null)
        {
            if (cancellationToken == null)
            {
                await semaphoreSlim.WaitAsync().ConfigureAwait(false);
                await func();
            }
            else
            {
                semaphoreSlim.Wait(cancellationToken.Value);
                if (!cancellationToken.Value.IsCancellationRequested) await func();
            }
            semaphoreSlim.Release();
        }

        public T Send<T>(Func<T> func, CancellationToken? cancellationToken = null)
        {
            T result = default;
            if (cancellationToken == null)
            {
                semaphoreSlim.Wait();
                result = func();
            }
            else
            {
                semaphoreSlim.Wait(cancellationToken.Value);
                if (!cancellationToken.Value.IsCancellationRequested) result = func();
            }
            semaphoreSlim.Release();
            return result;
        }

        public T Send<T>(Func<Task<T>> func, CancellationToken? cancellationToken = null)
        {
            T result = default;
            if (cancellationToken == null)
            {
                semaphoreSlim.Wait();
                result = func().Result;
            }
            else
            {
                semaphoreSlim.Wait(cancellationToken.Value);
                if (!cancellationToken.Value.IsCancellationRequested) result = func().Result;
            }
            semaphoreSlim.Release();
            return result;
        }

        public async Task SendAsync(Action action, CancellationToken? cancellationToken = null)
        {
            if (cancellationToken == null)
            {
                await semaphoreSlim.WaitAsync().ConfigureAwait(false);
                action();
            }
            else
            {
                await semaphoreSlim.WaitAsync(cancellationToken.Value).ConfigureAwait(false);
                if (!cancellationToken.Value.IsCancellationRequested) action();
            }
            semaphoreSlim.Release();
        }

        public async Task SendAsync(Func<Task> func, CancellationToken? cancellationToken = null)
        {
            if (cancellationToken == null)
            {
                await semaphoreSlim.WaitAsync().ConfigureAwait(false);
                await func().ConfigureAwait(false);
            }
            else
            {
                await semaphoreSlim.WaitAsync(cancellationToken.Value).ConfigureAwait(false);
                if (!cancellationToken.Value.IsCancellationRequested) await func().ConfigureAwait(false);
            }
            semaphoreSlim.Release();
        }

        public async Task<T> SendAsync<T>(Func<T> func, CancellationToken? cancellationToken = null)
        {
            T result = default;
            if (cancellationToken == null)
            {
                await semaphoreSlim.WaitAsync().ConfigureAwait(false);
                result = func();
            }
            else
            {
                await semaphoreSlim.WaitAsync(cancellationToken.Value).ConfigureAwait(false);
                if (!cancellationToken.Value.IsCancellationRequested) result = func();
            }
            semaphoreSlim.Release();
            return result;
        }

        public async Task<T> SendAsync<T>(Func<Task<T>> func, CancellationToken? cancellationToken = null)
        {
            T result = default;
            if (cancellationToken == null)
            {
                await semaphoreSlim.WaitAsync().ConfigureAwait(false);
                result = await func();
            }
            else
            {
                await semaphoreSlim.WaitAsync(cancellationToken.Value).ConfigureAwait(false);
                if (!cancellationToken.Value.IsCancellationRequested) result = await func();
            }
            semaphoreSlim.Release();
            return result;
        }
    }
}
