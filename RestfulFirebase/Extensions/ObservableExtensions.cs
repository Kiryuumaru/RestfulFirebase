using System;
using System.Reactive.Linq;

namespace RestfulFirebase.Extensions
{
    public static class ObservableExtensions
    {
        public static IObservable<T> RetryAfterDelay<T, TException>(
            this IObservable<T> source,
            TimeSpan dueTime,
            Func<TException, bool> retryOnError,
            int? retryCount = null)
            where TException: Exception
        {
            int attempt = 0;

            var pipeline = Observable.Defer(() =>
            {
                return ((++attempt == 1) ? source : source.DelaySubscription(dueTime))
                    .Select(item => new Tuple<bool, T, Exception>(true, item, null))
                    .Catch<Tuple<bool, T, Exception>, TException>(e => retryOnError(e)
                        ? Observable.Throw<Tuple<bool, T, Exception>>(e)
                        : Observable.Return(new Tuple<bool, T, Exception>(false, default(T), e)));
            });

            if (retryCount.HasValue)
            {
                pipeline = pipeline.Retry(retryCount.Value);
            }
            else
            {
                pipeline = pipeline.Retry();
            }

            return pipeline.SelectMany(t => t.Item1
                ? Observable.Return(t.Item2)
                : Observable.Throw<T>(t.Item3));
        }
    }
}
