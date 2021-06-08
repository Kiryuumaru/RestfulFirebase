using ObservableHelpers;
using RestfulFirebase.Database.Realtime;
using RestfulFirebase.Database.Streaming;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestfulFirebase.Database.Models
{
    public class FirebaseObjectDictionary<T> : ObservableDictionary<string, T>, IRealtimeModel
        where T : FirebaseObject
    {
        #region Properties

        public RealtimeInstance RealtimeInstance { get; private set; }

        public bool HasAttachedRealtime { get => RealtimeInstance != null; }

        public event EventHandler<RealtimeInstanceEventArgs> RealtimeAttached;
        public event EventHandler<RealtimeInstanceEventArgs> RealtimeDetached;
        public event EventHandler<WireErrorEventArgs> WireError;

        private Func<string, T> itemInitializer;

        #endregion

        #region Initializer

        public FirebaseObjectDictionary(Func<string, T> itemInitializer)
        {
            this.itemInitializer = itemInitializer;
        }

        #endregion

        #region Methods

        public virtual void AttachRealtime(RealtimeInstance realtimeInstance, bool invokeSetFirst)
        {
            VerifyNotDisposed();

            if (RealtimeInstance != null)
            {
                Unsubscribe();
                RealtimeInstance = null;
            }

            RealtimeInstance = realtimeInstance;

            Subscribe();

            lock (this)
            {
                List<KeyValuePair<string, T>> objs = this.ToList();
                var paths = RealtimeInstance.GetSubPaths().Select(i => Utils.UrlSeparate(i)[0]).ToList();

                foreach (var obj in objs)
                {
                    WireValue(obj.Key, obj.Value, invokeSetFirst);
                    paths.RemoveAll(i => i == obj.Key);
                }

                foreach (var path in paths)
                {
                    if (this.Any(i => i.Key == path)) continue;
                    var item = ObjectFactory(path);
                    if (item == null) continue;
                    WireValue(path, item, false);
                    Add(path, item);
                }
            }

            OnRealtimeAttached(new RealtimeInstanceEventArgs(realtimeInstance));
        }

        public virtual void DetachRealtime()
        {
            VerifyNotDisposed();

            Unsubscribe();
            var args = new RealtimeInstanceEventArgs(RealtimeInstance);
            RealtimeInstance = null;
            OnRealtimeDetached(args);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                DetachRealtime();
            }
            base.Dispose(disposing);
        }

        protected virtual void OnRealtimeAttached(RealtimeInstanceEventArgs args)
        {
            SynchronizationContextPost(delegate
            {
                RealtimeAttached?.Invoke(this, args);
            });
        }

        protected virtual void OnRealtimeDetached(RealtimeInstanceEventArgs args)
        {
            SynchronizationContextPost(delegate
            {
                RealtimeDetached?.Invoke(this, args);
            });
        }

        protected virtual void OnWireError(WireErrorEventArgs args)
        {
            SynchronizationContextPost(delegate
            {
                WireError?.Invoke(this, args);
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
                if (!value.IsNull()) value.SetNull();
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
                RealtimeInstance.DataChanges += RealtimeInstance_DataChanges;
                RealtimeInstance.Error += RealtimeInstance_Error;
            }
        }

        private void Unsubscribe()
        {
            VerifyNotDisposed();

            if (RealtimeInstance != null)
            {
                RealtimeInstance.DataChanges -= RealtimeInstance_DataChanges;
                RealtimeInstance.Error -= RealtimeInstance_Error;
            }
        }

        private void RealtimeInstance_DataChanges(object sender, DataChangesEventArgs e)
        {
            VerifyNotDisposed();

            if (!string.IsNullOrEmpty(e.Path))
            {
                var separated = Utils.UrlSeparate(e.Path);
                var key = separated[0];
                lock (this)
                {
                    KeyValuePair<string, T> obj = this.FirstOrDefault(i => i.Key == key);
                    var isNull = RealtimeInstance.Child(key).IsNull();
                    if (obj.Value == null && !isNull)
                    {
                        var item = ObjectFactory(key);
                        if (item == null) return;
                        WireValue(key, item, false);
                        Add(key, item);
                    }
                    else if (obj.Value != null && isNull)
                    {
                        Remove(key);
                    }
                }
            }
        }

        private void RealtimeInstance_Error(object sender, WireErrorEventArgs e)
        {
            VerifyNotDisposed();

            OnWireError(e);
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
