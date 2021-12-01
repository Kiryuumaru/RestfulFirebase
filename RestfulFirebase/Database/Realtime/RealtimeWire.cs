using Newtonsoft.Json.Linq;
using RestfulFirebase.Database.Query;
using RestfulFirebase.Database.Streaming;
using RestfulFirebase.Exceptions;
using RestfulFirebase.Local;
using RestfulFirebase.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RestfulFirebase.Database.Realtime
{
    /// <summary>
    /// The base query subscribing fluid implementations for firebase realtime database.
    /// </summary>
    public class RealtimeWire : RealtimeInstance
    {
        #region Properties

        /// <inheritdoc/>
        public override bool HasFirstStream => hasFirstStream;

        /// <inheritdoc/>
        public override bool Started => subscription != null;

        internal EventHandler<StreamObject> Next;

        private IDisposable subscription;
        private bool hasFirstStream;

        #endregion

        #region Initializers

        internal RealtimeWire(RestfulFirebaseApp app, IFirebaseQuery query, ILocalDatabase localDatabase)
            : base(app, query, localDatabase)
        {

        }

        #endregion

        #region Methods

        /// <summary>
        /// Start to subscribe the wire to the node.
        /// </summary>
        public void Start()
        {
            if (IsDisposed)
            {
                return;
            }

            if (subscription == null)
            {
                string uri = Query.GetAbsoluteUrl();
                subscription = new NodeStreamer(App, Query, OnNext, (s, e) => OnError(e.Url, e.Exception)).Run();
            }
        }

        /// <summary>
        /// Unsubscribe the wire to the node.
        /// </summary>
        public void Stop()
        {
            if (IsDisposed)
            {
                return;
            }

            if (subscription != null)
            {
                subscription.Dispose();
                subscription = null;
            }
        }

        ///// <summary>
        ///// Creates a <see cref="Task"/> that will complete when the wire has first stream.
        ///// </summary>
        ///// <param name="timeout">
        ///// The <see cref="TimeSpan"/> timeout of the created task.
        ///// </param>
        ///// <returns>
        ///// A <see cref="Task"/> that represents the fully sync status.
        ///// </returns>
        ///// <exception cref="DatabaseRealtimeWireNotStarted">
        ///// Throws when wire was not started.
        ///// </exception>
        //public Task<bool> WaitForFirstStream(TimeSpan timeout)
        //{
        //    return WaitForFirstStream(true, new CancellationTokenSource(timeout).Token);
        //}

        ///// <summary>
        ///// Creates a <see cref="Task"/> that will complete when the wire has first stream.
        ///// </summary>
        ///// <param name="cancellationToken">
        ///// The <see cref="CancellationToken"/> for the wait synced status.
        ///// </param>
        ///// <returns>
        ///// A <see cref="Task"/> that represents the fully sync status.
        ///// </returns>
        ///// <exception cref="DatabaseRealtimeWireNotStarted">
        ///// Throws when wire was not started.
        ///// </exception>
        //public Task<bool> WaitForFirstStream(CancellationToken cancellationToken)
        //{
        //    return WaitForFirstStream(true, cancellationToken);
        //}

        ///// <summary>
        ///// Creates a <see cref="Task"/> that will complete when the wire has first stream.
        ///// </summary>
        ///// <param name="cancelOnError">
        ///// Specify <c>true</c> whether the task will be cancelled on error; otherwise <c>false</c>.
        ///// </param>
        ///// <param name="timeout">
        ///// The <see cref="TimeSpan"/> timeout of the created task.
        ///// </param>
        ///// <returns>
        ///// A <see cref="Task"/> that represents the fully sync status.
        ///// </returns>
        ///// <exception cref="DatabaseRealtimeWireNotStarted">
        ///// Throws when wire was not started.
        ///// </exception>
        //public Task<bool> WaitForFirstStream(bool cancelOnError, TimeSpan timeout)
        //{
        //    return WaitForFirstStream(cancelOnError, new CancellationTokenSource(timeout).Token);
        //}

        ///// <summary>
        ///// Creates a <see cref="Task"/> that will complete when the wire has first stream.
        ///// </summary>
        ///// <param name="cancelOnError">
        ///// Specify <c>true</c> whether the task will be cancelled on error; otherwise <c>false</c>.
        ///// </param>
        ///// <param name="cancellationToken">
        ///// The <see cref="CancellationToken"/> for the wait synced status.
        ///// </param>
        ///// <returns>
        ///// A <see cref="Task"/> that represents the fully sync status.
        ///// </returns>
        ///// <exception cref="DatabaseRealtimeWireNotStarted">
        ///// Throws when wire was not started.
        ///// </exception>
        //public async Task<bool> WaitForFirstStream(bool cancelOnError = true, CancellationToken? cancellationToken = null)
        //{
        //    if (IsDisposed)
        //    {
        //        return false;
        //    }
        //    if (!Started)
        //    {
        //        throw new DatabaseRealtimeWireNotStarted(nameof(WaitForFirstStream));
        //    }

        //    bool cancel = false;
        //    void RealtimeInstance_Error(object sender, WireExceptionEventArgs e)
        //    {
        //        cancel = true;
        //    }
        //    if (cancelOnError)
        //    {
        //        Error += RealtimeInstance_Error;
        //    }
        //    async Task<bool> waitTask()
        //    {
        //        while (!HasFirstStream && !cancel && !(cancellationToken?.IsCancellationRequested ?? false))
        //        {
        //            try
        //            {
        //                if (cancellationToken.HasValue)
        //                {
        //                    await Task.Delay(App.Config.DatabaseRetryDelay, cancellationToken.Value).ConfigureAwait(false);
        //                }
        //                else
        //                {
        //                    await Task.Delay(App.Config.DatabaseRetryDelay).ConfigureAwait(false);
        //                }
        //            }
        //            catch { }
        //        }
        //        return HasFirstStream;
        //    }
        //    bool result = await Task.Run(waitTask).ConfigureAwait(false);
        //    if (cancelOnError)
        //    {
        //        Error -= RealtimeInstance_Error;
        //    }
        //    return result;
        //}

        private void OnNext(object sender, StreamObject streamObject)
        {
            if (IsDisposed)
            {
                return;
            }

            Next?.Invoke(sender, streamObject);

            string[] path = UrlUtilities.Separate(streamObject.Path);

            if (streamObject.JToken.Type == JTokenType.Null)
            {
                MakeSync(default(string), path);
            }
            else if (streamObject.JToken is JValue jValue)
            {
                MakeSync(jValue.ToString(), path);
            }
            else if (streamObject.JToken is JObject || streamObject.JToken is JArray)
            {
                IDictionary<string[], object> pairs = streamObject.JToken.GetFlatHierarchy();
                Dictionary<string[], string> values = new Dictionary<string[], string>(pairs.Count, PathEqualityComparer.Instance);
                if (path.Length == 0)
                {
                    foreach (KeyValuePair<string[], object> pair in pairs)
                    {
                        values.Add(pair.Key, pair.Value.ToString());
                    }
                }
                else
                {
                    foreach (KeyValuePair<string[], object> pair in pairs)
                    {
                        string[] subPath = new string[pair.Key.Length + path.Length];
                        Array.Copy(path, 0, subPath, 0, path.Length);
                        Array.Copy(pair.Key, 0, subPath, path.Length - 1, pair.Key.Length);
                        values.Add(subPath, pair.Value.ToString());
                    }
                }
                MakeSync(values, path);
            }

            if (!HasFirstStream)
            {
                hasFirstStream = true;
            }
        }

        #endregion

        #region RealtimeInstance Members

        /// <inheritdoc/>
        public override RealtimeInstance Clone()
        {
            if (IsDisposed)
            {
                return default;
            }

            var clone = new RealtimeWire(App, Query, LocalDatabase);
            clone.SyncOperation.SetContext(this);

            Next += clone.OnNext;
            Disposing += delegate
            {
                Next -= clone.OnNext;
                clone.Dispose();
            };

            return clone;
        }

        #endregion

        #region Disposable Members

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Stop();
            }
            base.Dispose(disposing);
        }

        #endregion
    }
}
