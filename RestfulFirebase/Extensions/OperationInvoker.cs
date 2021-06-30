using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RestfulFirebase.Extensions
{
    /// <summary>
    /// Provides wrapped SemaphoreSlim with dynamic TokenCount.
    /// </summary>
    public class OperationInvoker
    {
        #region Properties

        /// <summary>
        /// Gets or sets the concurrent token count.
        /// </summary>
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

        #endregion

        #region Initializers

        /// <summary>
        /// Creates new instance of <see cref="OperationInvoker"/> class.
        /// </summary>
        /// <param name="initialTokenCount">
        /// The initial concurrent token count.
        /// </param>
        public OperationInvoker(int initialTokenCount)
        {
            semaphoreSlim = new SemaphoreSlim(initialTokenCount);
            semaphoreSlimControl = new SemaphoreSlim(1, 1);
            ConcurrentTokenCount = initialTokenCount;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Executes <paramref name="action"/> without blocking the executing thread.
        /// </summary>
        /// <param name="action">
        /// The action to be executed.
        /// </param>
        /// <param name="cancellationToken">
        /// The cancellation token used for execution cancellation.
        /// </param>
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

        /// <summary>
        /// Executes <paramref name="func"/> without blocking the executing thread.
        /// </summary>
        /// <param name="func">
        /// The function to be executed.
        /// </param>
        /// <param name="cancellationToken">
        /// The cancellation token used for execution cancellation.
        /// </param>
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

        /// <summary>
        /// Executes <paramref name="action"/> and blocking the executing thread until the <paramref name="action"/> ended.
        /// </summary>
        /// <param name="action">
        /// The action to be executed.
        /// </param>
        /// <param name="cancellationToken">
        /// The cancellation token used for execution cancellation.
        /// </param>
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

        /// <summary>
        /// Executes <paramref name="func"/> and blocking the executing thread until the <paramref name="func"/> ended.
        /// </summary>
        /// <param name="func">
        /// The function to be executed.
        /// </param>
        /// <param name="cancellationToken">
        /// The cancellation token used for execution cancellation.
        /// </param>
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

        /// <summary>
        /// Executes <paramref name="func"/> that can return a value and blocking the executing thread until the <paramref name="func"/> ended.
        /// </summary>
        /// <param name="func">
        /// The function to be executed.
        /// </param>
        /// <param name="cancellationToken">
        /// The cancellation token used for execution cancellation.
        /// </param>
        /// <returns>
        /// The returned value of the <paramref name="func"/>.
        /// </returns>
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

        /// <summary>
        /// Executes <paramref name="func"/> that can return a value and blocking the executing thread until the <paramref name="func"/> ended.
        /// </summary>
        /// <param name="func">
        /// The function to be executed.
        /// </param>
        /// <param name="cancellationToken">
        /// The cancellation token used for execution cancellation.
        /// </param>
        /// <returns>
        /// The returned value of the <paramref name="func"/>.
        /// </returns>
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

        /// <summary>
        /// Executes <paramref name="action"/> that can return a value and blocking the executing thread until the <paramref name="action"/> ended.
        /// </summary>
        /// <param name="action">
        /// The action to be executed.
        /// </param>
        /// <param name="cancellationToken">
        /// The cancellation token used for execution cancellation.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> that represents a proxy for the task returned by <paramref name="action"/>.
        /// </returns>
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

        /// <summary>
        /// Executes <paramref name="func"/> that can return a value and blocking the executing thread until the <paramref name="func"/> ended.
        /// </summary>
        /// <param name="func">
        /// The function to be executed.
        /// </param>
        /// <param name="cancellationToken">
        /// The cancellation token used for execution cancellation.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> that represents a proxy for the task returned by <paramref name="func"/>.
        /// </returns>
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

        /// <summary>
        /// Executes <paramref name="func"/> that can return a value and blocking the executing thread until the <paramref name="func"/> ended.
        /// </summary>
        /// <param name="func">
        /// The function to be executed.
        /// </param>
        /// <param name="cancellationToken">
        /// The cancellation token used for execution cancellation.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> that represents a proxy for the task returned by <paramref name="func"/>.
        /// </returns>
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

        /// <summary>
        /// Executes <paramref name="func"/> that can return a value and blocking the executing thread until the <paramref name="func"/> ended.
        /// </summary>
        /// <param name="func">
        /// The function to be executed.
        /// </param>
        /// <param name="cancellationToken">
        /// The cancellation token used for execution cancellation.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> that represents a proxy for the task returned by <paramref name="func"/>.
        /// </returns>
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

        #endregion
    }
}
