using ObservableHelpers;
using RestfulFirebase.Database.Realtime;
using RestfulFirebase.Database.Streaming;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RestfulFirebase.Database.Models
{
    public class FirebaseObjectDictionary<T> : ObservableDictionary<string, T>, IRealtimeModelProxy
        where T : FirebaseObject
    {
        #region Properties

        internal RealtimeInstance RealtimeInstance { get; private set; }

        private Func<string, T> itemInitializer;

        #endregion

        #region Initializer

        public FirebaseObjectDictionary(Func<string, T> itemInitializer)
        {
            this.itemInitializer = itemInitializer;
        }

        #endregion

        #region Methods

        public void DetachRealtime()
        {
            Unsubscribe();
            RealtimeInstance = null;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Unsubscribe();
                RealtimeInstance = null;
            }
            base.Dispose(disposing);
        }

        protected virtual void WireValue(string key, T value, bool invokeSetFirst)
        {
            VerifyNotDisposed();

            if (invokeSetFirst) RealtimeInstance.Child(key).PutModel(value);
            else RealtimeInstance.Child(key).SubModel(value);
        }

        protected override (string key, T value) ValueFactory(string key, T value)
        {
            VerifyNotDisposed();

            if (RealtimeInstance != null)
            {
                if (value.RealtimeInstance == null)
                {
                    WireValue(key, value, true);
                }
                else if (value.RealtimeInstance.Parent != RealtimeInstance) throw new Exception("Item has different existing wire");
            }
            return (key, value);
        }

        protected override bool ValueRemove(string key, out T value)
        {
            VerifyNotDisposed();

            var result = base.ValueRemove(key, out value);
            if (result)
            {
                value.SetNull();
                value.DetachRealtime();
            }
            return result;
        }

        protected T ObjectFactory(string key)
        {
            VerifyNotDisposed();

            return itemInitializer?.Invoke((key));
        }

        private void Subscribe()
        {
            VerifyNotDisposed();

            if (RealtimeInstance != null)
            {
                RealtimeInstance.OnInternalChanges += RealtimeInstance_OnInternalChanges;
                RealtimeInstance.OnInternalError += RealtimeInstance_OnInternalError;
            }
        }

        private void Unsubscribe()
        {
            VerifyNotDisposed();

            if (RealtimeInstance != null)
            {
                RealtimeInstance.OnInternalChanges -= RealtimeInstance_OnInternalChanges;
                RealtimeInstance.OnInternalError -= RealtimeInstance_OnInternalError;
            }
        }

        private void RealtimeInstance_OnInternalChanges(object sender, DataChangesEventArgs e)
        {
            VerifyNotDisposed();

            if (!string.IsNullOrEmpty(e.Path))
            {
                var separated = Utils.UrlSeparate(e.Path);
                var key = separated[0];
                KeyValuePair<string, T> obj;
                lock (this)
                {
                    obj = this.FirstOrDefault(i => i.Key == key);
                }
                var isNull = RealtimeInstance.Child(key).IsNull();
                if (obj.Value == null && !isNull)
                {
                    var item = ObjectFactory(key);
                    if (item == null) return;
                    WireValue(key, item, false);
                    lock (this)
                    {
                        Add(key, item);
                    }
                }
                else if (obj.Value != null && isNull)
                {
                    lock (this)
                    {
                        Remove(key);
                    }
                }
            }
        }

        private void RealtimeInstance_OnInternalError(object sender, WireErrorEventArgs e)
        {
            VerifyNotDisposed();

            InvokeOnError(e.Exception);
        }

        void IRealtimeModelProxy.StartRealtime(RealtimeInstance realtimeInstance, bool invokeSetFirst)
        {
            VerifyNotDisposed();

            if (RealtimeInstance != null)
            {
                Unsubscribe();
                RealtimeInstance = null;
            }

            RealtimeInstance = realtimeInstance;

            Subscribe();

            List<KeyValuePair<string, T>> objs = new List<KeyValuePair<string, T>>();
            lock (this)
            {
                objs = this.ToList();
            }
            var paths = RealtimeInstance.GetSubPaths().Select(i => Utils.UrlSeparate(i)[0]).ToList();

            foreach (var obj in objs)
            {
                WireValue(obj.Key, obj.Value, invokeSetFirst);
                paths.RemoveAll(i => i == obj.Key);
            }

            foreach (var path in paths)
            {
                lock (this)
                {
                    if (this.Any(i => i.Key == path)) continue;
                }
                var item = ObjectFactory(path);
                if (item == null) continue;
                WireValue(path, item, false);
                lock (this)
                {
                    Add(path, item);
                }
            }
        }

        #endregion
    }

    public class FirebaseObjectDictionary : FirebaseObjectDictionary<FirebaseObject>
    {
        public FirebaseObjectDictionary()
            : base(new Func<string, FirebaseObject>(key => new FirebaseObject()))
        {

        }
    }
}
