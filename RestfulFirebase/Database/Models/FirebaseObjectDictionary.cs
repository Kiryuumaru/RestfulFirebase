using ObservableHelpers;
using RestfulFirebase.Database.Realtime;
using RestfulFirebase.Database.Streaming;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RestfulFirebase.Database.Models
{
    public class FirebaseObjectDictionary<T> : ObservableDictionary<string, T>, IRealtimeModel
        where T : FirebaseObject
    {
        #region Properties

        public bool HasAttachedRealtime { get => RealtimeInstance != null; }

        public event EventHandler<RealtimeInstanceEventArgs> OnRealtimeAttached;
        public event EventHandler<RealtimeInstanceEventArgs> OnRealtimeDetached;

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

        public void AttachRealtime(RealtimeInstance realtimeInstance, bool invokeSetFirst)
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

            InvokeOnRealtimeAttached(new RealtimeInstanceEventArgs(realtimeInstance));
        }

        public void DetachRealtime()
        {
            Unsubscribe();
            var args = new RealtimeInstanceEventArgs(RealtimeInstance);
            RealtimeInstance = null;
            InvokeOnRealtimeDetached(args);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                DetachRealtime();
            }
            base.Dispose(disposing);
        }

        protected void InvokeOnRealtimeAttached(RealtimeInstanceEventArgs args)
        {
            SynchronizationContextPost(delegate
            {
                OnRealtimeAttached?.Invoke(this, args);
            });
        }

        protected void InvokeOnRealtimeDetached(RealtimeInstanceEventArgs args)
        {
            SynchronizationContextPost(delegate
            {
                OnRealtimeDetached?.Invoke(this, args);
            });
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
