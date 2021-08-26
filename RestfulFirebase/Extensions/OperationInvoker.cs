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

        private SemaphoreSlim operationLock;
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
            operationLock = new SemaphoreSlim(initialTokenCount);
            ConcurrentTokenCount = initialTokenCount;
            lastTokenCount = initialTokenCount;
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
                await operationLock.WaitAsync().ConfigureAwait(false);
                action();
            }
            else
            {
                await operationLock.WaitAsync(cancellationToken.Value).ConfigureAwait(false);
                if (!cancellationToken.Value.IsCancellationRequested) action();
            }
            operationLock.Release();
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
                await operationLock.WaitAsync().ConfigureAwait(false);
                await func().ConfigureAwait(false);
            }
            else
            {
                await operationLock.WaitAsync(cancellationToken.Value).ConfigureAwait(false);
                if (!cancellationToken.Value.IsCancellationRequested) await func().ConfigureAwait(false);
            }
            operationLock.Release();
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
                operationLock.Wait();
                action();
            }
            else
            {
                operationLock.Wait(cancellationToken.Value);
                if (!cancellationToken.Value.IsCancellationRequested) action();
            }
            operationLock.Release();
        }

        /// <summary>
        /// Executes <paramref name="action"/> and blocking the executing thread until the <paramref name="action"/> ended.
        /// </summary>
        /// <param name="action">
        /// The action to be executed.
        /// </param>
        /// <param name="returnOnLockFree">
        /// Specify <c>true</c> if the method will return on lock free; otherwise <c>false</c>.
        /// </param>
        /// <param name="cancellationToken">
        /// The cancellation token used for execution cancellation.
        /// </param>
        public void Send(Action action, bool returnOnLockFree, CancellationToken? cancellationToken = null)
        {
            if (cancellationToken == null)
            {
                operationLock.Wait();
                if (returnOnLockFree)
                {
                    Task.Run(delegate
                    {
                        action();
                        operationLock.Release();
                    });
                }
                else
                {
                    action();
                    operationLock.Release();
                }
            }
            else
            {
                operationLock.Wait(cancellationToken.Value);
                if (!cancellationToken.Value.IsCancellationRequested)
                {
                    if (returnOnLockFree)
                    {
                        Task.Run(delegate
                        {
                            action();
                            operationLock.Release();
                        });
                    }
                    else
                    {
                        action();
                        operationLock.Release();
                    }
                }
                else
                {
                    operationLock.Release();
                }
            }
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
                operationLock.Wait();
                result = func();
            }
            else
            {
                operationLock.Wait(cancellationToken.Value);
                if (!cancellationToken.Value.IsCancellationRequested) result = func();
            }
            operationLock.Release();
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
                operationLock.Wait();
                result = func().Result;
            }
            else
            {
                operationLock.Wait(cancellationToken.Value);
                if (!cancellationToken.Value.IsCancellationRequested) result = func().Result;
            }
            operationLock.Release();
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
                await operationLock.WaitAsync().ConfigureAwait(false);
                action();
            }
            else
            {
                await operationLock.WaitAsync(cancellationToken.Value).ConfigureAwait(false);
                if (!cancellationToken.Value.IsCancellationRequested) action();
            }
            operationLock.Release();
        }

        /// <summary>
        /// Executes <paramref name="action"/> that can return a value and blocking the executing thread until the <paramref name="action"/> ended.
        /// </summary>
        /// <param name="action">
        /// The action to be executed.
        /// </param>
        /// <param name="returnOnLockFree">
        /// Specify <c>true</c> if the method will return on lock free; otherwise <c>false</c>.
        /// </param>
        /// <param name="cancellationToken">
        /// The cancellation token used for execution cancellation.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> that represents a proxy for the task returned by <paramref name="action"/>.
        /// </returns>
        public async Task SendAsync(Action action, bool returnOnLockFree, CancellationToken? cancellationToken = null)
        {
            if (cancellationToken == null)
            {
                await operationLock.WaitAsync().ConfigureAwait(false);
                if (returnOnLockFree)
                {
                    void onLockFree()
                    {
                        Task.Run(delegate
                        {
                            action();
                            operationLock.Release();
                        });
                    }
                    onLockFree();
                }
                else
                {
                    action();
                    operationLock.Release();
                }
            }
            else
            {
                await operationLock.WaitAsync(cancellationToken.Value).ConfigureAwait(false);
                if (!cancellationToken.Value.IsCancellationRequested)
                {
                    if (returnOnLockFree)
                    {
                        void onLockFree()
                        {
                            Task.Run(delegate
                            {
                                action();
                                operationLock.Release();
                            });
                        }
                        onLockFree();
                    }
                    else
                    {
                        action();
                        operationLock.Release();
                    }
                }
                else
                {
                    operationLock.Release();
                }
            }
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
                await operationLock.WaitAsync().ConfigureAwait(false);
                await func().ConfigureAwait(false);
            }
            else
            {
                await operationLock.WaitAsync(cancellationToken.Value).ConfigureAwait(false);
                if (!cancellationToken.Value.IsCancellationRequested) await func().ConfigureAwait(false);
            }
            operationLock.Release();
        }

        /// <summary>
        /// Executes <paramref name="func"/> that can return a value and blocking the executing thread until the <paramref name="func"/> ended.
        /// </summary>
        /// <param name="func">
        /// The function to be executed.
        /// </param>
        /// <param name="returnOnLockFree">
        /// Specify <c>true</c> if the method will return on lock free; otherwise <c>false</c>.
        /// </param>
        /// <param name="cancellationToken">
        /// The cancellation token used for execution cancellation.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> that represents a proxy for the task returned by <paramref name="func"/>.
        /// </returns>
        public async Task SendAsync(Func<Task> func, bool returnOnLockFree, CancellationToken? cancellationToken = null)
        {
            if (cancellationToken == null)
            {
                await operationLock.WaitAsync().ConfigureAwait(false);
                if (returnOnLockFree)
                {
                    async void onLockFree()
                    {
                        await func().ConfigureAwait(false);
                        operationLock.Release();
                    }
                    onLockFree();
                }
                else
                {
                    await func().ConfigureAwait(false);
                    operationLock.Release();
                }
            }
            else
            {
                await operationLock.WaitAsync(cancellationToken.Value).ConfigureAwait(false);
                if (!cancellationToken.Value.IsCancellationRequested)
                {
                    if (returnOnLockFree)
                    {
                        async void onLockFree()
                        {
                            await func().ConfigureAwait(false);
                            operationLock.Release();
                        }
                        onLockFree();
                    }
                    else
                    {
                        await func().ConfigureAwait(false);
                        operationLock.Release();
                    }
                }
                else
                {
                    operationLock.Release();
                }
            }
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
                await operationLock.WaitAsync().ConfigureAwait(false);
                result = func();
            }
            else
            {
                await operationLock.WaitAsync(cancellationToken.Value).ConfigureAwait(false);
                if (!cancellationToken.Value.IsCancellationRequested) result = func();
            }
            operationLock.Release();
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
                await operationLock.WaitAsync().ConfigureAwait(false);
                result = await func();
            }
            else
            {
                await operationLock.WaitAsync(cancellationToken.Value).ConfigureAwait(false);
                if (!cancellationToken.Value.IsCancellationRequested) result = await func();
            }
            operationLock.Release();
            return result;
        }

        private async void EvaluateTokenCount()
        {
            int currentToken = ConcurrentTokenCount;
            if (lastTokenCount < currentToken)
            {
                int tokenToRelease = currentToken - lastTokenCount;
                lastTokenCount = currentToken;
                operationLock.Release(tokenToRelease);
            }
            else if (lastTokenCount > currentToken)
            {
                int tokenToWait = lastTokenCount - currentToken;
                lastTokenCount = currentToken;
                for (int i = 0; i < tokenToWait; i++)
                {
                    await operationLock.WaitAsync();
                }
            }
        }

        #endregion
    }
}
