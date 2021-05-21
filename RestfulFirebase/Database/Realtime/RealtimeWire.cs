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
                var data = App.Database.OfflineDatabase.GetData(Query.GetAbsolutePath());
                var subData = App.Database.OfflineDatabase.GetSubDatas(Query.GetAbsolutePath());
                return subData.Count() + (data == null ? 0 : 1);
            }
        }

        public int SyncedDataCount
        {
            get
            {
                var data = App.Database.OfflineDatabase.GetData(Query.GetAbsolutePath());
                var subData = App.Database.OfflineDatabase.GetSubDatas(Query.GetAbsolutePath());
                return subData.Where(i => i.Changes == null).Count() + (data == null ? 0 : 1);
            }
        }

        public event EventHandler<DataChangesEventArgs> OnChanges;
        public event EventHandler<SyncEventArgs> OnSync;
        public event EventHandler<Exception> OnError;

        internal event EventHandler<DataChangesEventArgs> OnInternalChanges;
        internal event EventHandler<SyncEventArgs> OnInternalSync;
        internal event EventHandler<Exception> OnInternalError;

        private IDisposable subscription;
        private SynchronizationContext context = AsyncOperationManager.SynchronizationContext;

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

        public void MakeChanges(string path, string blob)
        {
            path = path.TrimStart('/');
            path = path.TrimEnd('/');
            if (string.IsNullOrEmpty(path)) return;
            var uri = Utils.CombineUrl(Query.GetAbsolutePath(), path);
            var dataHolder = new DataHolder(App, uri);
            if (dataHolder.MakeChanges(blob, err => OnPutError(dataHolder, err)))
            {
                InvokeOnChangesAndSync(uri);
            }
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
            var separatedPath = Utils.SeparateUrl(path);
            var affectedUris = new List<string>();
            var eventUri = Utils.CombineUrl(baseUri);
            affectedUris.Add(eventUri);
            for (int i = 0; i < separatedPath.Length; i++)
            {
                eventUri = Utils.CombineUrl(eventUri, separatedPath[i]);
                affectedUris.Add(eventUri);
            }
            foreach (var affectedUri in affectedUris.OrderByDescending(i => i.Length))
            {
                OnInternalChanges?.Invoke(this, new DataChangesEventArgs(affectedUri));
                context.Post(s =>
                {
                    OnChanges?.Invoke(this, new DataChangesEventArgs(affectedUri));
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
                var datas = App.Database.OfflineDatabase.GetDataAndSubDatas(streamObject.Uri);
                foreach (var data in datas)
                {
                    if (data?.MakeSync(null, err => OnPutError(data, err)) ?? false)
                    {
                        eventInvoked = true;
                        InvokeOnChangesAndSync(data.Uri);
                    }
                }
            }
            else if (streamObject.Data is SingleStreamData2 single)
            {
                // Delete multi
                var subDatas = App.Database.OfflineDatabase.GetSubDatas(streamObject.Uri);
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
            else if (streamObject.Data is MultiStreamData2 multi)
            {
                // Delete single
                var data = App.Database.OfflineDatabase.GetData(streamObject.Uri);
                if (data?.MakeSync(null, err => OnPutError(data, err)) ?? false)
                {
                    eventInvoked = true;
                    InvokeOnChangesAndSync(data.Uri);
                }

                var subDatas = App.Database.OfflineDatabase.GetSubDatas(streamObject.Uri);
                var descendants = multi.GetDescendants();
                var syncDatas = new List<(string path, string blob)>(descendants.Select(i => (Utils.CombineUrl(streamObject.Uri, i.path), i.blob)));
                var excluded = subDatas.Where(i => syncDatas.Any(j => j.path != i.Uri));

                // Delete excluded multi
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
                InvokeOnChangesAndSync(Query.GetAbsolutePath());
            }
            HasFirstStream = true;
        }

        #endregion
    }
}
