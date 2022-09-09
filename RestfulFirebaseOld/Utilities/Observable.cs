using System;

namespace RestfulFirebase.Utilities;

/// <summary>
/// Provides an operator for the push-based notifications mechanism <see cref="IObserver{T}"/>.
/// </summary>
public class Observable
{
    /// <summary>
    /// Creates new instance of the <see cref="Observable{T}"/> class.
    /// </summary>
    /// <typeparam name="T">
    /// The type of the push notification value.
    /// </typeparam>
    /// <param name="subscribe">
    /// The callback of the push-based notifications.
    /// </param>
    /// <returns>
    /// The created <see cref="Observable{T}"/> class.
    /// </returns>
    public static Observable<T> Create<T>(Func<IObserver<T>, IDisposable> subscribe)
    {
        return new Observable<T>(subscribe);
    }

    /// <summary>
    /// Creates new instance of <see cref="Observable"/> class.
    /// </summary>
    protected Observable()
    {

    }
}

/// <summary>
/// Provides an operator for the push-based notifications mechanism <see cref="IObserver{T}"/>.
/// </summary>
/// <typeparam name="T">
/// The type of the push notification value.
/// </typeparam>
public class Observable<T> : Observable
{
    private readonly Func<IObserver<T>, IDisposable> subscribe;

    /// <summary>
    /// Creates new instance of the <see cref="Observable{T}"/> class.
    /// </summary>
    /// <param name="subscribe">
    /// The callback of the push-based notifications.
    /// </param>
    public Observable(Func<IObserver<T>, IDisposable> subscribe)
    {
        this.subscribe = subscribe;
    }

    /// <summary>
    /// Subscribe the provided callback to the mechanism.
    /// </summary>
    /// <param name="onNext">
    /// Callback for pushed notifications.
    /// </param>
    /// <param name="onError">
    /// Callback for the error received.
    /// </param>
    /// <param name="onComplete">
    /// Callback for the completion of the push notification mechanism.
    /// </param>
    /// <returns>
    /// The disposable subscription of the mechanism.
    /// </returns>
    public IDisposable Subscribe(Action<T> onNext, Action<Exception>? onError = null, Action? onComplete = null)
    {
        var observer = new Observer()
        {
            OnCompletedAction = onComplete,
            OnErrorAction = onError,
            OnNextAction = onNext
        };
        return subscribe.Invoke(observer);
    }

    internal class Observer : IObserver<T>
    {
        public Action? OnCompletedAction;
        public Action<Exception>? OnErrorAction;
        public Action<T>? OnNextAction;

        public void OnCompleted() => OnCompletedAction?.Invoke();
        public void OnError(Exception error) => OnErrorAction?.Invoke(error);
        public void OnNext(T value) => OnNextAction?.Invoke(value);
    }
}
