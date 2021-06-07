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
    public class RealtimeWire : RealtimeInstance
    {
        #region Properties

        public bool HasFirstStream { get; private set; }

        public bool Started => subscription != null;

        private IDisposable subscription;

        #endregion

        #region Initializers

        public RealtimeWire(RestfulFirebaseApp app, IFirebaseQuery query)
            : base(app, query)
        {

        }

        #endregion

        #region Methods

        public void Start()
        {
            VerifyNotDisposed();

            string uri = Query.GetAbsolutePath();
            subscription = new NodeStreamer(App, Query, OnNext, (s, e) => OnError(uri, e)).Run();
        }

        public void Stop()
        {
            VerifyNotDisposed();

            subscription?.Dispose();
            subscription = null;
        }

        public async Task<bool> WaitForFirstStream(TimeSpan timeout)
        {
            VerifyNotDisposed();

            return await Task.Run(async delegate
            {
                while (!HasFirstStream) { await Task.Delay(100); }
                return true;
            }).WithTimeout(timeout, false);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Stop();
                UnsubscribeToParent();
            }
            base.Dispose(disposing);
        }

        private void OnNext(object sender, StreamObject streamObject)
        {
            if (IsDisposed) return;

            var urisToInvoke = new List<string>() { streamObject.Uri };

            if (streamObject.Data is null)
            {
                // Delete all
                var subDatas = App.Database.OfflineDatabase.GetDatas(streamObject.Uri, true, true, Query.GetAbsolutePath());
                foreach (var subData in subDatas)
                {
                    if (subData?.MakeSync(null, err => OnPutError(subData, err)) ?? false)
                    {
                        if (!urisToInvoke.Any(i => Utils.UrlCompare(i, subData.Uri))) urisToInvoke.Add(subData.Uri);
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
                        if (!urisToInvoke.Any(i => Utils.UrlCompare(i, subData.Uri))) urisToInvoke.Add(subData.Uri);
                    }
                }

                // Make single
                var data = App.Database.OfflineDatabase.GetData(streamObject.Uri) ?? new DataHolder(App, streamObject.Uri);
                if (data.MakeSync(single.Blob, err => OnPutError(data, err)))
                {
                    if (!urisToInvoke.Any(i => Utils.UrlCompare(i, data.Uri))) urisToInvoke.Add(data.Uri);
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
                        if (!urisToInvoke.Any(i => Utils.UrlCompare(i, subData.Uri))) urisToInvoke.Add(subData.Uri);
                    }
                }

                // Make multi
                foreach (var syncData in syncDatas)
                {
                    var subData = subDatas.FirstOrDefault(i => i.Uri == syncData.path);
                    if (subData == null) subData = new DataHolder(App, syncData.path);
                    if (subData?.MakeSync(syncData.blob, err => OnPutError(subData, err)) ?? false)
                    {
                        if (!urisToInvoke.Any(i => Utils.UrlCompare(i, subData.Uri))) urisToInvoke.Add(subData.Uri);
                    }
                }
            }
            OnDataChanges(urisToInvoke.ToArray());
            if (!HasFirstStream) HasFirstStream = true;
        }

        #endregion
    }
}
