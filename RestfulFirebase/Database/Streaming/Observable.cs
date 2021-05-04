using System;
using System.Collections.Generic;
using System.Text;

namespace RestfulFirebase.Database.Streaming
{
    public class Observable
    {
        public static Observable<T> Create<T>(Func<IObserver<T>, IDisposable> subscribe)
        {
            return new Observable<T>(subscribe);
        }

        public Observable()
        {

        }
    }
    public class Observable<T> : Observable
    {
        Func<IObserver<T>, IDisposable> subscribe;

        public Observable(Func<IObserver<T>, IDisposable> subscribe)
        {
            this.subscribe = subscribe;
        }

        private class Observer : IObserver<T>
        {
            public Action OnCompletedAction;
            public Action<Exception> OnErrorAction;
            public Action<T> OnNextAction;

            public void OnCompleted()
            {
                OnCompletedAction?.Invoke();
            }

            public void OnError(Exception error)
            {
                OnErrorAction?.Invoke(error);
            }

            public void OnNext(T value)
            {
                OnNextAction?.Invoke(value);
            }
        }

        public IDisposable Subscribe(Action<T> onNext, Action<Exception> onError = null, Action onComplete = null)
        {
            var observer = new Observer()
            {
                OnCompletedAction = onComplete,
                OnErrorAction = onError,
                OnNextAction = onNext
            };
            return subscribe?.Invoke(observer);
        }
    }
}
