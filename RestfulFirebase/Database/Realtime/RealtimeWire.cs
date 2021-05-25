using ObservableHelpers;
using RestfulFirebase.Database.Offline;
using RestfulFirebase.Database.Query;
using RestfulFirebase.Database.Streaming;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;

namespace RestfulFirebase.Database.Realtime
{
    public class RealtimeWire
    {
        #region Properties

        public RestfulFirebaseApp App { get; }

        public IFirebaseQuery Query { get; }

        public bool HasFirstStream { get; private set; }

        public int TotalDataCount
        {
            get
            {
                var uri = Query.GetAbsolutePath();
                return App.Database.OfflineDatabase.GetDatas(uri, true).Count();
            }
        }

        public int SyncedDataCount
        {
            get
            {
                var uri = Query.GetAbsolutePath();
                return App.Database.OfflineDatabase.GetDatas(uri, true).Where(i => i.Changes == null).Count();
            }
        }

        public event EventHandler<DataChangesEventArgs> OnChanges;
        public event EventHandler<SyncEventArgs> OnSync;
        public event EventHandler<Exception> OnError;

        public event EventHandler<DataChangesEventArgs> OnInternalChanges;
        public event EventHandler<SyncEventArgs> OnInternalSync;
        public event EventHandler<Exception> OnInternalError;

        private readonly SynchronizationContext context = AsyncOperationManager.SynchronizationContext;
        private IDisposable subscription;

        #endregion

        #region Initializers

        public RealtimeWire(RestfulFirebaseApp app, IFirebaseQuery query)
        {
            App = app;
            Query = query;
        }

        #endregion

        #region Methods

        public void Start()
        {
            subscription = new NodeStreamer(App, Query, OnNext, OnError).Run();
        }

        public void Stop()
        {
            subscription?.Dispose();
            subscription = null;
        }

        public bool SetBlob(string blob, string path = null)
        {
            var hasChanges = false;

            path = path?.Trim();
            path = path?.Trim('/');
            var uri = string.IsNullOrEmpty(path) ? Utils.UrlCombine(Query.GetAbsolutePath()) : Utils.UrlCombine(Query.GetAbsolutePath(), path);
            
            // Delete related changes
            var subDatas = App.Database.OfflineDatabase.GetDatas(uri, false, true);
            foreach (var subData in subDatas)
            {
                if (subData.DeleteChanges()) hasChanges = true;
            }

            // Make changes
            var dataHolder = new DataHolder(App, uri);
            if (dataHolder.MakeChanges(blob, err => OnPutError(dataHolder, err)))
            {
                hasChanges = true;
                InvokeOnChangesAndSync(uri);
            }

            return hasChanges;
        }

        public string GetBlob(string path = null)
        {
            path = path?.TrimStart('/');
            path = path?.TrimEnd('/');
            var uri = string.IsNullOrEmpty(path) ? Utils.UrlCombine(Query.GetAbsolutePath()) : Utils.UrlCombine(Query.GetAbsolutePath(), path);
            var dataHolder = new DataHolder(App, uri);
            return dataHolder.Blob;
        }

        public IEnumerable<string> GetPaths(string path = null)
        {
            var uri = string.IsNullOrEmpty(path) ? Query.GetAbsolutePath() : Utils.UrlCombine(Query.GetAbsolutePath(), path);
            return App.Database.OfflineDatabase.GetSubUris(uri, true);
        }

        protected void InvokeOnChangesAndSync(string uri)
        {
            InvokeOnChanges(uri);
            InvokeOnSync();
        }

        protected void InvokeOnChanges(string uri)
        {
            uri = uri.EndsWith("/") ? uri : uri + "/";
            var baseUri = Query.GetAbsolutePath();
            if (!uri.StartsWith(baseUri)) return;
            var path = uri.Replace(baseUri, "");
            var separatedPath = Utils.UrlSeparate(path);
            var affectedPaths = new List<string>();
            var eventPath = "";
            affectedPaths.Add(eventPath);
            for (int i = 0; i < separatedPath.Length; i++)
            {
                if (string.IsNullOrEmpty(eventPath)) eventPath = Utils.UrlCombine(separatedPath[i]);
                else eventPath = Utils.UrlCombine(eventPath, separatedPath[i]);
                affectedPaths.Add(eventPath);
            }
            foreach (var affectedPath in affectedPaths.OrderByDescending(i => i.Length))
            {
                OnInternalChanges?.Invoke(this, new DataChangesEventArgs(baseUri, affectedPath));
                context.Post(s =>
                {
                    OnChanges?.Invoke(this, new DataChangesEventArgs(baseUri, affectedPath));
                }, null);
            }
        }

        protected void InvokeOnSync()
        {
            OnInternalSync?.Invoke(this, new SyncEventArgs(TotalDataCount, SyncedDataCount));
            context.Post(s =>
            {
                OnSync?.Invoke(this, new SyncEventArgs(TotalDataCount, SyncedDataCount));
            }, null);
        }

        protected void InvokeOnError(Exception exception)
        {
            OnInternalError?.Invoke(this, exception);
            context.Post(s =>
            {
                OnError?.Invoke(this, exception);
            }, null);
        }

        private void OnPutError(DataHolder holder, RetryExceptionEventArgs err)
        {
            var hasChanges = false;
            if (err.Exception is FirebaseException ex)
            {
                if (ex.Reason == FirebaseExceptionReason.DatabaseUnauthorized)
                {
                    if (holder.Sync == null)
                    {
                        if (holder.Delete()) hasChanges = true;
                    }
                    else
                    {
                        if (holder.DeleteChanges()) hasChanges = true;
                    }
                }
            }
            InvokeOnError(err.Exception);
            if (hasChanges)
            {
                InvokeOnChangesAndSync(holder.Uri);
            }
        }

        private void OnNext(object sender, StreamObject streamObject)
        {
            var eventInvoked = false;
            if (streamObject.Data is null)
            {
                // Delete all
                var datas = App.Database.OfflineDatabase.GetDatas(streamObject.Uri, true, true, Query.GetAbsolutePath());
                foreach (var data in datas)
                {
                    if (data?.MakeSync(null, err => OnPutError(data, err)) ?? false)
                    {
                        eventInvoked = true;
                        InvokeOnChangesAndSync(data.Uri);
                    }
                }
            }
            else if (streamObject.Data is SingleStreamData single)
            {
                // Delete related
                var subDatas = App.Database.OfflineDatabase.GetDatas(streamObject.Uri, false, true, Query.GetAbsolutePath());
                foreach (var subData in subDatas)
                {
                    if (subData?.MakeSync(null, err => OnPutError(subData, err)) ?? false)
                    {
                        eventInvoked = true;
                        InvokeOnChangesAndSync(subData.Uri);
                    }
                }

                // Make single
                var data = App.Database.OfflineDatabase.GetData(streamObject.Uri) ?? new DataHolder(App, streamObject.Uri);
                if (data.MakeSync(single.Blob, err => OnPutError(data, err)))
                {
                    eventInvoked = true;
                    InvokeOnChangesAndSync(data.Uri);
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
                    if (subData?.MakeSync(null, err => OnPutError(subData, err)) ?? false)
                    {
                        eventInvoked = true;
                        InvokeOnChangesAndSync(subData.Uri);
                    }
                }

                // Make multi
                foreach (var syncData in syncDatas)
                {
                    var subData = subDatas.FirstOrDefault(i => i.Uri == syncData.path);
                    if (subData == null) subData = new DataHolder(App, syncData.path);
                    if (subData?.MakeSync(syncData.blob, err => OnPutError(subData, err)) ?? false)
                    {
                        eventInvoked = true;
                        InvokeOnChangesAndSync(subData.Uri);
                    }
                }
            }
            if (!eventInvoked)
            {
                InvokeOnSync();
            }
            HasFirstStream = true;
        }

        #endregion
    }
}
