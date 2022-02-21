using System;
using System.Threading.Tasks;

namespace RestfulFirebase.Utilities;

/// <summary>
/// Provides <see cref="Task"/> extensions
/// </summary>
public static class TaskExtensions
{
    /// <summary>
    /// Executes <paramref name="source"/> and throw any catched aggregated exceptions.
    /// </summary>
    /// <param name="source">
    /// The <see cref="Task"/> to execute.
    /// </param>
    /// <returns>
    /// A <see cref="Task"/> that represents a proxy for the <see cref="Task"/> returned by <paramref name="source"/>.
    /// </returns>
    public static async Task WithAggregateException(this Task source)
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

    /// <summary>
    /// Executes <paramref name="task"/> with timeout.
    /// </summary>
    /// <param name="task">
    /// The <see cref="Task"/> to be executed.
    /// </param>
    /// <param name="timeoutInMilliseconds">
    /// The milliseconds timeout of the provided <paramref name="task"/>.
    /// </param>
    /// <returns>
    /// A <see cref="Task"/> that represents a proxy for the task returned by <paramref name="task"/>.
    /// </returns>
    public static async Task WithTimeout(this Task task, int timeoutInMilliseconds)
    {
        await Task.WhenAny(task, Task.Delay(timeoutInMilliseconds)).ConfigureAwait(false);
    }

    /// <summary>
    /// Executes <paramref name="task"/> that can return a value with timeout.
    /// </summary>
    /// <param name="task">
    /// The <see cref="Task"/> to be executed.
    /// </param>
    /// <param name="timeoutInMilliseconds">
    /// The milliseconds timeout of the provided <paramref name="task"/>.
    /// </param>
    /// <param name="defaultValue">
    /// The default return value if timeout.
    /// </param>
    /// <returns>
    /// A <see cref="Task"/> that represents a proxy for the <see cref="Task"/> returned by <paramref name="task"/>.
    /// </returns>
    public static async Task<T?> WithTimeout<T>(this Task<T> task, int timeoutInMilliseconds, T? defaultValue = default)
    {
        T? returnValue = defaultValue;
        var retTask = await Task.WhenAny(Task.Run(async delegate
        {
            returnValue = await task;
        }), Task.Delay(timeoutInMilliseconds)).ConfigureAwait(false);
        return returnValue;
    }

    /// <summary>
    /// Executes <paramref name="task"/> with timeout.
    /// </summary>
    /// <param name="task">
    /// The <see cref="Task"/> to be executed.
    /// </param>
    /// <param name="timeout">
    /// The <see cref="TimeSpan"/> timeout of the provided <paramref name="task"/>.
    /// </param>
    /// <returns>
    /// A <see cref="Task"/> that represents a proxy for the <see cref="Task"/> returned by <paramref name="task"/>.
    /// </returns>
    public static Task WithTimeout(this Task task, TimeSpan timeout) => WithTimeout(task, (int)timeout.TotalMilliseconds);

    /// <summary>
    /// Executes <paramref name="task"/> that can return a value with timeout.
    /// </summary>
    /// <param name="task">
    /// The <see cref="Task"/> to be executed.
    /// </param>
    /// <param name="timeout">
    /// The <see cref="TimeSpan"/> timeout of the provided <paramref name="task"/>.
    /// </param>
    /// <param name="defaultValue">
    /// The default return value if timeout.
    /// </param>
    /// <returns>
    /// A <see cref="Task"/> that represents a proxy for the <see cref="Task"/> returned by <paramref name="task"/>.
    /// </returns>
    public static Task<T?> WithTimeout<T>(this Task<T> task, TimeSpan timeout, T? defaultValue = default) => WithTimeout(task, (int)timeout.TotalMilliseconds, defaultValue);

    /// <summary>
    /// Executes <paramref name="task"/> asynchronously provided with the <paramref name="onException"/> callback for catched exceptions.
    /// </summary>
    /// <param name="task">
    /// The <see cref="Task"/> to be executed.
    /// </param>
    /// <param name="onException">
    /// The callback for catched exceptions.
    /// </param>
    /// <param name="continueOnCapturedContext">
    /// Configures an awaiter used to await this <paramref name="task"/>.
    /// </param>
    public static async void SafeFireAndForget(this Task task, Action<Exception>? onException = null, bool continueOnCapturedContext = false)
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
