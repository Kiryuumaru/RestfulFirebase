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

        protected RealtimeInstance(RestfulFirebaseApp app, RealtimeInstance parent, string path)
        {
            App = app;
            Parent = parent;
            Query = parent.Query.Child(path);
            SubscribeToParent();
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

        public RealtimeInstance Child(string path)
        {
            VerifyNotDisposed();

            return new RealtimeInstance(App, this, path);
        }

        public int GetTotalDataCount()
        {
            VerifyNotDisposed();

            var uri = Query.GetAbsolutePath();
            return App.Database.OfflineDatabase.GetDatas(uri, true).Count();
        }

        public int GetSyncedDataCount()
        {
            VerifyNotDisposed();

            var uri = Query.GetAbsolutePath();
            return App.Database.OfflineDatabase.GetDatas(uri, true).Where(i => i.Changes == null).Count();
        }

        public async Task WaitForSynced()
        {
            VerifyNotDisposed();

            await Task.Run(async delegate
            {
                while (!IsSynced) { await Task.Delay(500); }
            });
        }

        public async Task<bool> WaitForSynced(TimeSpan timeout)
        {
            VerifyNotDisposed();

            return await Task.Run(async delegate
            {
                while (!IsSynced) { await Task.Delay(500); }
                return true;
            }).WithTimeout(timeout, false);
        }

        public bool SetBlob(string blob)
        {
            VerifyNotDisposed();

            var hasChanges = false;

            var affectedUris = new List<string>();

            var uri = Query.GetAbsolutePath();

            // Delete related changes
            var subDatas = App.Database.OfflineDatabase.GetDatas(uri, false, true);
            foreach (var subData in subDatas)
            {
                if (subData.MakeChanges(blob, err => OnPutError(subData, err)))
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
            VerifyNotDisposed();

            var uri = Query.GetAbsolutePath();
            var dataHolder = new DataHolder(App, uri);
            return dataHolder.Blob;
        }

        public IEnumerable<string> GetSubPaths()
        {
            VerifyNotDisposed();

            var uri = Query.GetAbsolutePath();
            return App.Database.OfflineDatabase.GetSubUris(uri, false).Select(i => i.Replace(uri, "").Trim('/')).Where(i => !string.IsNullOrEmpty(i));
        }

        public IEnumerable<string> GetSubUris()
        {
            VerifyNotDisposed();

            var uri = Query.GetAbsolutePath();
            return App.Database.OfflineDatabase.GetSubUris(uri, false);
        }

        public bool IsNull()
        {
            VerifyNotDisposed();

            var uri = Query.GetAbsolutePath();
            return App.Database.OfflineDatabase.GetDatas(uri, true).All(i => i.Blob == null);
        }

        public T PutModel<T>(T model)
            where T : IRealtimeModel
        {
            VerifyNotDisposed();

            model.AttachRealtime(this, true);
            return model;
        }

        public T SubModel<T>(T model)
            where T : IRealtimeModel
        {
            VerifyNotDisposed();

            model.AttachRealtime(this, false);
            return model;
        }

        public override string ToString()
        {
            VerifyNotDisposed();

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
            VerifyNotDisposed();

            if (Parent != null)
            {
                Parent.DataChanges += Parent_DataChanges;
                Parent.Error += Parent_Error;
            }
        }

        protected void UnsubscribeToParent()
        {
            VerifyNotDisposed();

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
            string baseUri = Query.GetAbsolutePath();
            if (e.Uri.StartsWith(baseUri))
            {
                var path = e.Uri.Replace(baseUri, "");
                SelfDataChanges(new DataChangesEventArgs(baseUri, path));
            }
        }

        private void Parent_Error(object sender, WireErrorEventArgs e)
        {
            string baseUri = Query.GetAbsolutePath();
            if (e.Uri.StartsWith(baseUri))
            {
                SelfError(e);
            }
        }

        private void SelfDataChanges(DataChangesEventArgs e)
        {
            SynchronizationContextPost(delegate
            {
                DataChanges?.Invoke(this, e);
            });
        }

        private void SelfError(WireErrorEventArgs e)
        {
            SynchronizationContextPost(delegate
            {
                Error?.Invoke(this, e);
            });
        }

        #endregion
    }
}
