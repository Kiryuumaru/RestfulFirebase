using ObservableHelpers;
using RestfulFirebase.Database.Models;
using RestfulFirebase.Database.Offline;
using RestfulFirebase.Database.Query;
using RestfulFirebase.Database.Streaming;
using RestfulFirebase.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RestfulFirebase.Database.Realtime
{
    public class RealtimeInstance : SyncContext
    {
        #region Properties

        public RestfulFirebaseApp App { get; }

        public IFirebaseQuery Query { get; }

        public RealtimeInstance Parent { get; }

        public event EventHandler<DataChangesEventArgs> DataChanges;
        public event EventHandler<WireErrorEventArgs> Error;

        public bool IsSynced { get => GetTotalDataCount() == GetSyncedDataCount(); }

        #endregion

        #region Initializers

        protected RealtimeInstance(RestfulFirebaseApp app, IFirebaseQuery query)
        {
            App = app;
            Query = query;
        }

        protected RealtimeInstance(RestfulFirebaseApp app, RealtimeInstance parent, IFirebaseQuery query)
           : this(app, query)
        {
            Parent = parent;

            Parent.Disposing += Parent_Disposing;
            SubscribeToParent();
        }

        protected RealtimeInstance(RestfulFirebaseApp app, RealtimeInstance parent, string path)
           : this(app, parent, parent.Query.Child(path))
        {

        }

        #endregion

        #region Methods

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                UnsubscribeToParent();
            }
            base.Dispose(disposing);
        }

        public virtual RealtimeInstance Clone()
        {
            if (IsDisposed)
            {
                return default;
            }

            var clone = new RealtimeInstance(App, Parent, Query);
            clone.SyncOperation.SetContext(this);

            return clone;
        }

        public bool HasChild(string path)
        {
            if (IsDisposed)
            {
                return false;
            }

            var uri = Utils.UrlCombine(Query.GetAbsolutePath().Trim('/'), path);
            return App.Database.OfflineDatabase.GetDatas(uri, true).Any(i => i.Blob != null);
        }

        public RealtimeInstance Child(string path)
        {
            if (IsDisposed)
            {
                return default;
            }

            var childWire = new RealtimeInstance(App, this, path);
            childWire.SyncOperation.SetContext(this);

            return childWire;
        }

        public int GetTotalDataCount()
        {
            if (IsDisposed)
            {
                return 0;
            }

            var uri = Query.GetAbsolutePath();
            return App.Database.OfflineDatabase.GetDatas(uri, true).Count();
        }

        public int GetSyncedDataCount()
        {
            if (IsDisposed)
            {
                return 0;
            }

            var uri = Query.GetAbsolutePath();
            return App.Database.OfflineDatabase.GetDatas(uri, true).Where(i => i.Changes == null).Count();
        }

        public async Task WaitForSynced()
        {
            if (IsDisposed)
            {
                return;
            }

            await Task.Run(async delegate
            {
                while (!IsSynced) { await Task.Delay(500).ConfigureAwait(false); }
            }).ConfigureAwait(false);
        }

        public async Task<bool> WaitForSynced(TimeSpan timeout)
        {
            if (IsDisposed)
            {
                return false;
            }

            return await Task.Run(async delegate
            {
                while (!IsSynced) { await Task.Delay(500).ConfigureAwait(false); }
                return true;
            }).WithTimeout(timeout, false).ConfigureAwait(false);
        }

        public bool SetBlob(string blob)
        {
            if (IsDisposed)
            {
                return false;
            }

            var hasChanges = false;

            var affectedUris = new List<string>();

            var uri = Query.GetAbsolutePath();

            // Delete related changes
            var subDatas = App.Database.OfflineDatabase.GetDatas(uri, false, true);
            foreach (var subData in subDatas)
            {
                if (subData.MakeChanges(null, err => OnPutError(subData, err)))
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
                OnDataChanges(affectedUris.ToArray());
            }

            return hasChanges;
        }

        public bool SetNull()
        {
            return SetBlob(null);
        }

        public string GetBlob()
        {
            if (IsDisposed)
            {
                return default;
            }

            var uri = Query.GetAbsolutePath();
            var dataHolder = new DataHolder(App, uri);
            return dataHolder.Blob;
        }

        public IEnumerable<string> GetSubPaths()
        {
            if (IsDisposed)
            {
                return null;
            }

            var uri = Query.GetAbsolutePath();
            return App.Database.OfflineDatabase.GetSubUris(uri, false).Select(i => i.Replace(uri, "").Trim('/')).Where(i => !string.IsNullOrEmpty(i));
        }

        public IEnumerable<string> GetSubUris()
        {
            if (IsDisposed)
            {
                return null;
            }

            var uri = Query.GetAbsolutePath();
            return App.Database.OfflineDatabase.GetSubUris(uri, false);
        }

        public bool IsNull()
        {
            if (IsDisposed)
            {
                return true;
            }

            var uri = Query.GetAbsolutePath();
            return App.Database.OfflineDatabase.GetDatas(uri, true).All(i => i.Blob == null);
        }

        public T PutModel<T>(T model)
            where T : IRealtimeModel
        {
            if (IsDisposed)
            {
                return model;
            }

            model.AttachRealtime(this, true);
            return model;
        }

        public T SubModel<T>(T model)
            where T : IRealtimeModel
        {
            if (IsDisposed)
            {
                return model;
            }

            model.AttachRealtime(this, false);
            return model;
        }

        public override string ToString()
        {
            return Query.GetAbsolutePath();
        }

        protected void OnDataChanges(params string[] uris)
        {
            if (IsDisposed)
            {
                return;
            }

            if (Parent == null)
            {
                var affectedPaths = new List<string>();
                var baseUri = Query.GetAbsolutePath();
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
                    SelfDataChanges(new DataChangesEventArgs(baseUri, affectedPath));
                }
            }
            else
            {
                Parent.OnDataChanges(uris);
            }
        }

        protected void OnError(string uri, Exception exception)
        {
            if (IsDisposed)
            {
                return;
            }

            if (Parent == null)
            {
                SelfError(new WireErrorEventArgs(uri, exception));
            }
            else
            {
                Parent.OnError(uri, exception);
            }
        }

        protected void SubscribeToParent()
        {
            if (IsDisposed)
            {
                return;
            }

            if (Parent != null)
            {
                Parent.DataChanges += Parent_DataChanges;
                Parent.Error += Parent_Error;
            }
        }

        protected void UnsubscribeToParent()
        {
            if (IsDisposed)
            {
                return;
            }

            if (Parent != null)
            {
                Parent.DataChanges -= Parent_DataChanges;
                Parent.Error -= Parent_Error;
            }
        }

        internal void OnPutError(DataHolder holder, RetryExceptionEventArgs err)
        {
            if (IsDisposed)
            {
                return;
            }

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
            OnError(holder.Uri, err.Exception);
            if (hasChanges)
            {
                OnDataChanges(holder.Uri);
            }
        }

        private void Parent_DataChanges(object sender, DataChangesEventArgs e)
        {
            if (IsDisposed)
            {
                return;
            }

            string baseUri = Query.GetAbsolutePath().Trim('/');
            if (e.Uri.StartsWith(baseUri))
            {
                var path = e.Uri.Replace(baseUri, "");
                SelfDataChanges(new DataChangesEventArgs(baseUri, path));
            }
        }

        private void Parent_Error(object sender, WireErrorEventArgs e)
        {
            if (IsDisposed)
            {
                return;
            }

            string baseUri = Query.GetAbsolutePath().Trim('/');
            if (e.Uri.StartsWith(baseUri))
            {
                SelfError(e);
            }
        }

        private void Parent_Disposing(object sender, EventArgs e)
        {
            if (IsDisposed)
            {
                return;
            }

            Dispose();
        }

        private void SelfDataChanges(DataChangesEventArgs e)
        {
            ContextPost(delegate
            {
                DataChanges?.Invoke(this, e);
            });
        }

        private void SelfError(WireErrorEventArgs e)
        {
            ContextPost(delegate
            {
                Error?.Invoke(this, e);
            });
        }

        #endregion
    }
}
