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
        public FirebaseQuery Query { get; }
        public bool InvokeSetFirst { get; private set; }
        public bool HasFirstStream { get; private set; }
        public bool IsWritting { get; private set; }
        public bool HasPendingWrite { get; private set; }

        public event Action OnStart;
        public event Action OnStop;
        public event Func<StreamObject, bool> OnStream;
        public event EventHandler<DataChangesEventArgs> OnDataChanges;
        public event Action OnFirstStream;

        public int TotalDataCount
        {
            get
            {
                var data = App.Database.OfflineDatabase.GetData(Query.GetAbsolutePath());
                var subData = App.Database.OfflineDatabase.GetSubDatas(Query.GetAbsolutePath());
                return subData.Count() + (data.Exist ? 1 : 0);
            }
        }

        public int SyncedDataCount
        {
            get
            {
                var data = App.Database.OfflineDatabase.GetData(Query.GetAbsolutePath());
                var subData = App.Database.OfflineDatabase.GetSubDatas(Query.GetAbsolutePath());
                return subData.Where(i => i.Changes == null).Count() + (data.Exist ? 1 : 0);
            }
        }

        #endregion

        #region Initializers

        public static RealtimeWire CreateFromParent(RestfulFirebaseApp app, RealtimeWire parentWire, string key, bool invokeSetFirst)
        {
            return new RealtimeWire(
                app,
                parentWire,
                new ChildQuery(app, parentWire.Query, () => key),
                invokeSetFirst);
        }

        public static RealtimeWire CreateFromQuery(RestfulFirebaseApp app, FirebaseQuery query, bool invokeSetFirst)
        {
            return new RealtimeWire(
                app,
                null,
                query,
                invokeSetFirst);
        }

        protected RealtimeWire(RestfulFirebaseApp app, RealtimeWire parentWire, FirebaseQuery query, bool invokeSetFirst)
        {
            App = app;
            ParentWire = parentWire;
            Query = query;
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
            // Bug: Late push event still acknowledged
            //if (IsWritting && HasPendingWrite) return false;
            //{
            //    if (isStreamWaiting) return false;
            //    isStreamWaiting = true;
            //    while (IsWritting && HasPendingWrite) { }
            //    isStreamWaiting = false;
            //}
            hasChanges = OnStream?.Invoke(streamObjectBuffer) ?? false;
            if (!HasFirstStream)
            {
                HasFirstStream = true;
                OnFirstStream?.Invoke();
            }
            InvokeDataChanges();
            return hasChanges;
        }

        public RealtimeWire Child(string key, bool invokeSetFirst)
        {
            return RealtimeWire.CreateFromParent(App, this, key, invokeSetFirst);
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
                    else if (err.Exception is FirebaseDatabaseException firEx && firEx.InnerException is HttpRequestException)
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

        public async Task WaitForFirstStream(TimeSpan timeout)
        {
            await Task.WhenAny(
                Task.Run(async delegate { while (!HasFirstStream) { await Task.Delay(500); } }),
                Task.Delay(timeout));
        }

        #endregion
    }

    public class RealtimeWire<T> : RealtimeWire
        where T : IRealtimeModel
    {
        #region Methods

        public T Model { get; private set; }

        #endregion

        #region Initializers

        public static RealtimeWire<T> CreateFromParent(RestfulFirebaseApp app, RealtimeWire parentWire, string key, T model, bool invokeSetFirst)
        {
            return new RealtimeWire<T>(
                app,
                parentWire,
                new ChildQuery(app, parentWire.Query, () => key),
                model,
                invokeSetFirst);
        }

        public static RealtimeWire<T> CreateFromQuery(RestfulFirebaseApp app, FirebaseQuery query, T model, bool invokeSetFirst)
        {
            return new RealtimeWire<T>(
                app,
                null,
                query,
                model,
                invokeSetFirst);
        }

        private RealtimeWire(RestfulFirebaseApp app, RealtimeWire parentWire, FirebaseQuery query, T model, bool invokeSetFirst)
            : base(app, parentWire, query, invokeSetFirst)
        {
            Model = model;
        }

        #endregion

        #region Methods

        public override void Start()
        {
            Model.MakeRealtime(this);
            InvokeStart();
            Subscription = Observable
                .Create<StreamObject>(observer => new NodeStreamer(observer, Query, (s, e) => Model.OnError(e)).Run())
                .Subscribe(streamObject => { InvokeStream(streamObject); });
        }

        #endregion
    }
}
