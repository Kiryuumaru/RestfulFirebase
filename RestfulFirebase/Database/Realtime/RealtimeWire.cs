using RestfulFirebase.Auth;
using RestfulFirebase.Database.Models;
using RestfulFirebase.Database.Offline;
using RestfulFirebase.Database.Query;
using RestfulFirebase.Database.Streaming;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RestfulFirebase.Database.Realtime
{
    public class RealtimeWire : IDisposable
    {
        #region Properties

        private string jsonToPut;
        //private bool isStreamWaiting;
        private StreamObject streamObjectBuffer;

        protected IDisposable Subscription;

        public RestfulFirebaseApp App { get; }
        public RealtimeWire ParentWire { get; }
        public FirebaseQuery ParentQuery { get; }
        public FirebaseQuery Query { get; }
        public string Key { get; }
        public bool InvokeSetFirst { get; private set; }
        public bool HasFirstStream { get; private set; }
        public bool IsWritting { get; private set; }
        public bool HasPendingWrite { get; private set; }

        public event Action OnStart;
        public event Action OnStop;
        public event Func<StreamObject, bool> OnStream;
        public event EventHandler<DataChangesEventArgs> OnDataChanges;

        public int TotalDataCount
        {
            get
            {
                return GetSubDatas().Count() + (GetData().Exist ? 1 : 0);
            }
        }

        public int SyncedDataCount
        {
            get
            {
                return GetSubDatas().Where(i => i.Changes == null).Count() + (GetData().Exist ? 1 : 0);
            }
        }

        #endregion

        #region Initializers

        public RealtimeWire(RestfulFirebaseApp app, FirebaseQuery parentQuery, string key, bool invokeSetFirst)
        {
            App = app;
            ParentWire = null;
            ParentQuery = parentQuery;
            Query = new ChildQuery(parentQuery.App, parentQuery, () => key);
            Key = key;
            InvokeSetFirst = invokeSetFirst;
        }

        public RealtimeWire(RestfulFirebaseApp app, RealtimeWire parentWire, string key, bool invokeSetFirst)
        {
            App = app;
            ParentWire = parentWire;
            ParentQuery = parentWire.Query;
            Query = new ChildQuery(parentWire.App, parentWire.Query, () => key);
            Key = key;
            InvokeSetFirst = invokeSetFirst;
        }

        #endregion

        #region Methods

        private void InvokeDataChanges() => OnDataChanges?.Invoke(this, new DataChangesEventArgs(TotalDataCount, SyncedDataCount));

        public void InvokeStart() => OnStart?.Invoke();

        public void InvokeStop() => OnStop?.Invoke();

        public bool InvokeStream(StreamObject streamObject)
        {
            var hasChanges = false;
            streamObjectBuffer = streamObject;
            // FIX LATER
            //if (IsWritting && HasPendingWrite) return false;
            //{
            //    if (isStreamWaiting) return false;
            //    isStreamWaiting = true;
            //    while (IsWritting && HasPendingWrite) { }
            //    isStreamWaiting = false;
            //}
            hasChanges = OnStream?.Invoke(streamObjectBuffer) ?? false;
            if (!HasFirstStream) HasFirstStream = true;
            InvokeDataChanges();
            return hasChanges;
        }

        public RealtimeWire Child(string key, bool invokeSetFirst)
        {
            return new RealtimeWire(App, this, key, invokeSetFirst);
        }

        public DataNode GetData()
        {
            return App.Database.OfflineDatabase.GetData(Query.GetAbsolutePath());
        }

        public IEnumerable<DataNode> GetSubDatas()
        {
            return App.Database.OfflineDatabase.GetSubDatas(Query.GetAbsolutePath());
        }

        public async void Put(string json, Action<RetryExceptionEventArgs> onError)
        {
            InvokeDataChanges();
            ParentWire?.InvokeDataChanges();

            jsonToPut = json;
            HasPendingWrite = true;
            if (IsWritting) return;
            IsWritting = true;
            while (HasPendingWrite)
            {
                HasPendingWrite = false;
                await Query.Put(() => jsonToPut, null, err =>
                {
                    Type exType = err.Exception.GetType();
                    if (err.Exception is OfflineModeException)
                    {
                        err.Retry = true;
                    }
                    else if (err.Exception is TaskCanceledException)
                    {
                        err.Retry = true;
                    }
                    else if (err.Exception is HttpRequestException)
                    {
                        err.Retry = true;
                    }
                    else if (err.Exception is FirebaseAuthException)
                    {
                        err.Retry = true;
                    }
                    else
                    {
                        onError(err);
                    }
                });
            }
            IsWritting = false;
        }

        public virtual void Start()
        {
            InvokeStart();
        }

        public void Dispose()
        {
            Subscription?.Dispose();
            OnStop?.Invoke();
        }

        #endregion
    }

    public class RealtimeWire<T> : RealtimeWire
        where T : IRealtimeModel
    {
        public T Model { get; private set; }

        public RealtimeWire(RestfulFirebaseApp app, FirebaseQuery parentQuery, string key, T model, bool invokeSetFirst)
            : base(app, parentQuery, key, invokeSetFirst)
        {
            Model = model;
        }

        public RealtimeWire(RestfulFirebaseApp app, RealtimeWire parentWire, string key, T model, bool invokeSetFirst)
            : base(app, parentWire, key, invokeSetFirst)
        {
            Model = model;
        }

        public override void Start()
        {
            Model.MakeRealtime(this);
            InvokeStart();
            Subscription = Observable
                .Create<StreamObject>(observer => new NodeStreamer(observer, Query, (s, e) => Model.OnError(e)).Run())
                .Subscribe(streamObject => { InvokeStream(streamObject); });
        }
    }
}
