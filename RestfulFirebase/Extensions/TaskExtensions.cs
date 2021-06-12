using System;
using System.Threading.Tasks;

namespace RestfulFirebase.Extensions
{
    internal static class TaskExtensions
    {
        internal static async Task WithAggregateException(this Task source)
        {
            try
            {
                await source.ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw source.Exception ?? ex;
            }
        }

        internal static async Task WithTimeout(this Task task, int timeoutInMilliseconds)
        {
            await Task.WhenAny(task, Task.Delay(timeoutInMilliseconds)).ConfigureAwait(false);
        }

        internal static async Task<T> WithTimeout<T>(this Task<T> task, int timeoutInMilliseconds, T defaultValue = default)
        {
            var retTask = await Task.WhenAny(task, Task.Delay(timeoutInMilliseconds)).ConfigureAwait(false);
            return retTask is Task<T> ? task.Result : defaultValue;
        }

        internal static Task WithTimeout(this Task task, TimeSpan timeout) => WithTimeout(task, (int)timeout.TotalMilliseconds);
        
        internal static Task<T> WithTimeout<T>(this Task<T> task, TimeSpan timeout, T defaultValue = default) => WithTimeout(task, (int)timeout.TotalMilliseconds, defaultValue);

        internal static async void SafeFireAndForget(this Task task, Action<Exception> onException = null, bool continueOnCapturedContext = false)
        {
            try
            {
                await task.ConfigureAwait(continueOnCapturedContext);
            }
            catch (Exception ex) when (onException != null)
            {
                onException.Invoke(ex);
            }
        }
    }
}
