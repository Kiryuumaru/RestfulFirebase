using RestfulFirebase.Database.Models;
using RestfulFirebase.Database.Offline;
using RestfulFirebase.Database.Query;
using RestfulFirebase.Database.Streaming;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;

namespace RestfulFirebase.Database.Realtime
{
    public class RealtimeInstance : IDisposable
    {
        #region Properties

        public RestfulFirebaseApp App { get; }

        public IFirebaseQuery Query { get; }

        public RealtimeInstance Parent { get; }

        public event EventHandler<DataChangesEventArgs> OnChanges;
        public event EventHandler<WireErrorEventArgs> OnError;

        public event EventHandler<DataChangesEventArgs> OnInternalChanges;
        public event EventHandler<WireErrorEventArgs> OnInternalError;

        private readonly SynchronizationContext context = AsyncOperationManager.SynchronizationContext;

        #endregion

        #region Initializers

        protected RealtimeInstance(RestfulFirebaseApp app, IFirebaseQuery query)
        {
            App = app;
            Query = query;
        }

        protected RealtimeInstance(RestfulFirebaseApp app, RealtimeInstance parent, string path)
        {
            App = app;
            Parent = parent;
            Query = parent.Query.Child(path);
            SubscribeToParent();
        }

        #endregion

        #region Methods

        public virtual void Dispose()
        {
            UnsubscribeToParent();
        }

        public RealtimeInstance Child(string path)
        {
            return new RealtimeInstance(App, this, path);
        }

        public int GetTotalDataCount()
        {
            var uri = Query.GetAbsolutePath();
            return App.Database.OfflineDatabase.GetDatas(uri, true).Count();
        }

        public int GetSyncedDataCount()
        {
            var uri = Query.GetAbsolutePath();
            return App.Database.OfflineDatabase.GetDatas(uri, true).Where(i => i.Changes == null).Count();
        }

        public bool SetBlob(string blob)
        {
            var hasChanges = false;

            var affectedUris = new List<string>();

            var uri = Query.GetAbsolutePath();

            // Delete related changes
            var subDatas = App.Database.OfflineDatabase.GetDatas(uri, false, true);
            foreach (var subData in subDatas)
            {
                if (subData.DeleteChanges())
                {
                    hasChanges = true;
                    affectedUris.Add(subData.Uri);
                }
            }

            // Make changes
            var dataHolder = new DataHolder(App, uri);
            if (dataHolder.MakeChanges(blob, err => OnPutError(dataHolder, err)))
            {
                hasChanges = true;
                affectedUris.Add(uri);
            }

            if (hasChanges)
            {
                InvokeOnChanges(affectedUris.ToArray());
            }

            return hasChanges;
        }

        public string GetBlob()
        {
            var uri = Query.GetAbsolutePath();
            var dataHolder = new DataHolder(App, uri);
            return dataHolder.Blob;
        }

        public IEnumerable<string> GetSubPaths()
        {
            var uri = Query.GetAbsolutePath();
            return App.Database.OfflineDatabase.GetSubUris(uri, false).Select(i => i.Replace(uri, "").Trim('/')).Where(i => !string.IsNullOrEmpty(i));
        }

        public IEnumerable<string> GetSubUris()
        {
            var uri = Query.GetAbsolutePath();
            return App.Database.OfflineDatabase.GetSubUris(uri, false);
        }

        public bool SetNull()
        {
            var hasChanges = false;

            var affectedUris = new List<string>();

            var uri = Query.GetAbsolutePath();

            // Delete related changes
            var subDatas = App.Database.OfflineDatabase.GetDatas(uri, false, true);
            foreach (var subData in subDatas)
            {
                if (subData.DeleteChanges())
                {
                    hasChanges = true;
                    affectedUris.Add(subData.Uri);
                }
            }

            // Make changes
            var dataHolder = new DataHolder(App, uri);
            if (dataHolder.MakeChanges(null, err => OnPutError(dataHolder, err)))
            {
                hasChanges = true;
                affectedUris.Add(uri);
            }

            if (hasChanges)
            {
                InvokeOnChanges(affectedUris.ToArray());
            }

            return hasChanges;
        }

        public bool IsNull()
        {
            var uri = Query.GetAbsolutePath();
            return App.Database.OfflineDatabase.GetDatas(uri, true).All(i => i.Blob == null);
        }

        public T PutModel<T>(T model)
            where T : IRealtimeModel
        {
            var modelProxy = (IRealtimeModelProxy)model;
            modelProxy.StartRealtime(this, true);
            return model;
        }

        public T SubModel<T>(T model)
            where T : IRealtimeModel
        {
            var modelProxy = (IRealtimeModelProxy)model;
            modelProxy.StartRealtime(this, false);
            return model;
        }

        public override string ToString()
        {
            return Query.GetAbsolutePath();
        }

        internal void OnPutError(DataHolder holder, RetryExceptionEventArgs err)
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
            InvokeOnError(holder.Uri, err.Exception);
            if (hasChanges)
            {
                InvokeOnChanges(holder.Uri);
            }
        }

        protected void InvokeOnChanges(params string[] uris)
        {
            if (Parent == null)
            {
                var affectedPaths = new List<string>();
                var baseUri = Query.GetAbsolutePath();
                var totalDataCount = GetTotalDataCount();
                var syncedDataCount = GetSyncedDataCount();
                affectedPaths.Add("");
                foreach (var u in uris)
                {
                    var uri = u.EndsWith("/") ? u : u + "/";
                    if (!uri.StartsWith(baseUri)) continue;
                    var path = uri.Replace(baseUri, "");
                    var separatedPath = Utils.UrlSeparate(path);
                    var eventPath = "";
                    for (int i = 0; i < separatedPath.Length; i++)
                    {
                        if (string.IsNullOrEmpty(eventPath)) eventPath = Utils.UrlCombine(separatedPath[i]);
                        else eventPath = Utils.UrlCombine(eventPath, separatedPath[i]);
                        if (!affectedPaths.Any(affectedPath => Utils.UrlCompare(affectedPath, eventPath))) affectedPaths.Add(eventPath);
                    }
                }
                foreach (var affectedPath in affectedPaths.OrderByDescending(i => i.Length))
                {
                    SelfChanges(new DataChangesEventArgs(baseUri, affectedPath, totalDataCount, syncedDataCount));
                }
            }
            else
            {
                Parent.InvokeOnChanges(uris);
            }
        }

        protected void InvokeOnError(string uri, Exception exception)
        {
            if (Parent == null)
            {
                SelfError(new WireErrorEventArgs(uri, exception));
            }
            else
            {
                Parent.InvokeOnError(uri, exception);
            }
        }

        protected void SubscribeToParent()
        {
            if (Parent != null)
            {
                Parent.OnInternalChanges += Parent_OnInternalChanges;
                Parent.OnInternalError += Parent_OnInternalError;
            }
        }

        protected void UnsubscribeToParent()
        {
            if (Parent != null)
            {
                Parent.OnInternalChanges -= Parent_OnInternalChanges;
                Parent.OnInternalError -= Parent_OnInternalError;
            }
        }

        private void Parent_OnInternalChanges(object sender, DataChangesEventArgs e)
        {
            string baseUri = Query.GetAbsolutePath();
            if (e.Uri.StartsWith(baseUri))
            {
                var path = e.Uri.Replace(baseUri, "");
                SelfChanges(new DataChangesEventArgs(baseUri, path, GetTotalDataCount(), GetSyncedDataCount()));
            }
        }

        private void Parent_OnInternalError(object sender, WireErrorEventArgs e)
        {
            string baseUri = Query.GetAbsolutePath();
            if (e.Uri.StartsWith(baseUri))
            {
                SelfError(e);
            }
        }

        private void SelfChanges(DataChangesEventArgs e)
        {
            OnInternalChanges?.Invoke(this, e);
            context.Post(s =>
            {
                OnChanges?.Invoke(this, e);
            }, null);
        }

        private void SelfError(WireErrorEventArgs e)
        {
            OnInternalError?.Invoke(this, e);
            context.Post(s =>
            {
                OnError?.Invoke(this, e);
            }, null);
        }

        #endregion
    }
}
