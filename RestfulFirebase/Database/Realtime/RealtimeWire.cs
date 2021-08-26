using ObservableHelpers;
using RestfulFirebase.Database.Models;
using RestfulFirebase.Database.Offline;
using RestfulFirebase.Database.Query;
using RestfulFirebase.Database.Streaming;
using RestfulFirebase.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
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

        /// <summary>
        /// Gets <c>true</c> whether the wire has first stream since creation; otherwise, <c>false</c>.
        /// </summary>
        public bool HasFirstStream { get; private set; }

        /// <summary>
        /// Gets <c>true</c> whether the wire has started the node subscription; otherwise, <c>false</c>.
        /// </summary>
        public bool Started => subscription != null;

        internal EventHandler<StreamObject> Next;

        private IDisposable subscription;

        #endregion

        #region Initializers

        internal RealtimeWire(RestfulFirebaseApp app, IFirebaseQuery query)
            : base(app, query)
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

            string uri = Query.GetAbsolutePath();
            subscription = new NodeStreamer(App, Query, OnNext, (s, e) => OnError(uri, e)).Run();
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

            subscription?.Dispose();
            subscription = null;
        }

        /// <inheritdoc/>
        public override RealtimeInstance Clone()
        {
            if (IsDisposed)
            {
                return default;
            }

            var clone = new RealtimeWire(App, Query);
            clone.SyncOperation.SetContext(this);
            clone.EvaluateData();
            Next += clone.OnNext;
            Disposing += delegate
            {
                Next -= clone.OnNext;
                clone.Dispose();
            };

            return clone;
        }

        /// <summary>
        /// Creates a <see cref="Task"/> that will complete when the wire has first stream.
        /// </summary>
        /// <param name="cancelOnError">
        /// Specify <c>true</c> whether the task will be cancelled on error; otherwise <c>false</c>.
        /// </param>
        /// <param name="cancellationToken">
        /// The <see cref="CancellationToken"/> for the wait synced status.
        /// </param>
        /// <param name="timeout">
        /// The <see cref="TimeSpan"/> timeout of the created task.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> that represents the fully sync status.
        /// </returns>
        public async Task<bool> WaitForFirstStream(bool cancelOnError = false, TimeSpan? timeout = null, CancellationToken? cancellationToken = null)
        {
            if (IsDisposed)
            {
                return false;
            }

            bool cancel = false;
            void RealtimeInstance_Error(object sender, WireExceptionEventArgs e)
            {
                cancel = true;
            }
            if (cancelOnError)
            {
                Error += RealtimeInstance_Error;
            }
            async Task<bool> waitTask()
            {
                while (!HasFirstStream && !cancel && !(cancellationToken?.IsCancellationRequested ?? false))
                {
                    try
                    {
                        if (cancellationToken.HasValue)
                        {
                            await Task.Delay(500, cancellationToken.Value).ConfigureAwait(false);
                        }
                        else
                        {
                            await Task.Delay(500).ConfigureAwait(false);
                        }
                    }
                    catch { }
                }
                return HasFirstStream;
            }
            bool result = timeout.HasValue
                ? await Task.Run(waitTask).WithTimeout(timeout.Value, false).ConfigureAwait(false)
                : await Task.Run(waitTask).ConfigureAwait(false);
            if (cancelOnError)
            {
                Error -= RealtimeInstance_Error;
            }
            return result;
        }

        /// <summary>
        /// Creates a <see cref="Task"/> that will complete when the wire has first stream.
        /// </summary>
        /// <param name="cancelOnError">
        /// Specify <c>true</c> whether the task will be cancelled on error; otherwise <c>false</c>.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> that represents the fully sync status.
        /// </returns>
        public Task<bool> WaitForFirstStream(bool cancelOnError)
        {
            return WaitForFirstStream(cancelOnError: cancelOnError);
        }

        /// <summary>
        /// Creates a <see cref="Task"/> that will complete when the wire has first stream.
        /// </summary>
        /// <param name="timeout">
        /// The <see cref="TimeSpan"/> timeout of the created task.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> that represents the fully sync status.
        /// </returns>
        public Task<bool> WaitForFirstStream(TimeSpan timeout)
        {
            return WaitForFirstStream(timeout: timeout);
        }

        /// <summary>
        /// Creates a <see cref="Task"/> that will complete when the wire has first stream.
        /// </summary>
        /// <param name="cancellationToken">
        /// The <see cref="CancellationToken"/> for the wait synced status.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> that represents the fully sync status.
        /// </returns>
        public Task<bool> WaitForFirstStream(CancellationToken cancellationToken)
        {
            return WaitForFirstStream(cancellationToken: cancellationToken);
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Stop();
            }
            base.Dispose(disposing);
        }

        private void OnNext(object sender, StreamObject streamObject)
        {
            if (IsDisposed)
            {
                return;
            }

            Next?.Invoke(sender, streamObject);

            var urisToInvoke = new List<string>() { streamObject.Uri };

            if (streamObject.Data is null)
            {
                // Delete all
                var subDatas = App.Database.OfflineDatabase.GetDatas(streamObject.Uri, true, true, Query.GetAbsolutePath());
                foreach (var subData in subDatas)
                {
                    if (MakeSync(subData, null))
                    {
                        if (!urisToInvoke.Any(i => Utils.UrlCompare(i, subData.Uri)))
                        {
                            urisToInvoke.Add(subData.Uri);
                        }
                    }
                }
            }
            else if (streamObject.Data is SingleStreamData single)
            {
                // Delete related
                var subDatas = App.Database.OfflineDatabase.GetDatas(streamObject.Uri, false, true, Query.GetAbsolutePath());
                foreach (var subData in subDatas)
                {
                    if (MakeSync(subData, null))
                    {
                        if (!urisToInvoke.Any(i => Utils.UrlCompare(i, subData.Uri)))
                        {
                            urisToInvoke.Add(subData.Uri);
                        }
                    }
                }

                // Make single
                var data = App.Database.OfflineDatabase.GetData(streamObject.Uri);
                if (MakeSync(data, single.Blob))
                {
                    if (!urisToInvoke.Any(i => Utils.UrlCompare(i, data.Uri)))
                    {
                        urisToInvoke.Add(data.Uri);
                    }
                }
            }
            else if (streamObject.Data is MultiStreamData multi)
            {
                var subDatas = App.Database.OfflineDatabase.GetDatas(streamObject.Uri, true, true, Query.GetAbsolutePath());
                var descendants = multi.GetDescendants();
                var syncDatas = new List<(string path, string blob)>(descendants.Select(i => (Utils.UrlCombine(streamObject.Uri, i.path), i.blob)));

                // Delete related
                var excluded = subDatas.Where(i => !syncDatas.Any(j => Utils.UrlCompare(j.path, i.Uri)));
                foreach (var subData in excluded)
                {
                    if (MakeSync(subData, null))
                    {
                        if (!urisToInvoke.Any(i => Utils.UrlCompare(i, subData.Uri)))
                        {
                            urisToInvoke.Add(subData.Uri);
                        }
                    }
                }

                // Make multi
                foreach (var syncData in syncDatas)
                {
                    var subData = subDatas.FirstOrDefault(i => i.Uri == syncData.path);
                    if (subData == null)
                    {
                        subData = App.Database.OfflineDatabase.GetData(syncData.path);
                    }
                    if (MakeSync(subData, syncData.blob))
                    {
                        if (!urisToInvoke.Any(i => Utils.UrlCompare(i, subData.Uri)))
                        {
                            urisToInvoke.Add(subData.Uri);
                        }
                    }
                }
            }
            OnDataChanges(urisToInvoke.ToArray());
            if (!HasFirstStream) HasFirstStream = true;
        }

        #endregion
    }
}
