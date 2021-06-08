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
    public class FirebasePropertyDictionary<T> : ObservableDictionary<string, T>, IRealtimeModel
        where T : FirebaseProperty
    {
        #region Properties

        public bool HasAttachedRealtime { get => RealtimeInstance != null; }

        public event EventHandler<RealtimeInstanceEventArgs> RealtimeAttached;
        public event EventHandler<RealtimeInstanceEventArgs> RealtimeDetached;
        public event EventHandler<WireErrorEventArgs> WireError;

        internal RealtimeInstance RealtimeInstance { get; private set; }

        private Func<string, T> itemInitializer;

        #endregion

        #region Initializer

        public FirebasePropertyDictionary(Func<string, T> itemInitializer)
        {
            this.itemInitializer = itemInitializer;
        }

        #endregion

        #region Methods

        public virtual void AttachRealtime(RealtimeInstance realtimeInstance, bool invokeSetFirst)
        {
            if (RealtimeInstance != null)
            {
                Unsubscribe();
                RealtimeInstance = null;
            }

            RealtimeInstance = realtimeInstance;

            Subscribe();

            List<KeyValuePair<string, T>> props = new List<KeyValuePair<string, T>>();
            lock (this)
            {
                props = this.ToList();
            }
            var paths = RealtimeInstance.GetSubPaths().Select(i => Utils.UrlSeparate(i)[0]).ToList();

            foreach (var prop in props)
            {
                WireValue(prop.Key, prop.Value, invokeSetFirst);
                paths.RemoveAll(i => i == prop.Key);
            }

            foreach (var path in paths)
            {
                lock (this)
                {
                    if (this.Any(i => i.Key == path)) continue;
                }
                var item = PropertyFactory((path));
                if (item == null) continue;
                WireValue(path, item, false);
                lock (this)
                {
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

        public async Task WaitForSynced()
        {
            VerifyNotDisposed();

            if (RealtimeInstance == null) throw new Exception("Model not wired to realtime wire");

            await RealtimeInstance.WaitForSynced();
        }

        public async Task<bool> WaitForSynced(TimeSpan timeout)
        {
            VerifyNotDisposed();

            if (RealtimeInstance == null) throw new Exception("Model not wired to realtime wire");

            return await RealtimeInstance.WaitForSynced(timeout);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                DetachRealtime();
            }
            base.Dispose(disposing);
        }

        protected void OnRealtimeAttached(RealtimeInstanceEventArgs args)
        {
            SynchronizationContextSend(delegate
            {
                RealtimeAttached?.Invoke(this, args);
            });
        }

        protected void OnRealtimeDetached(RealtimeInstanceEventArgs args)
        {
            SynchronizationContextSend(delegate
            {
                RealtimeDetached?.Invoke(this, args);
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

        protected T PropertyFactory(string key)
        {
            VerifyNotDisposed();

            return itemInitializer?.Invoke(key);
        }

        protected virtual void OnWireError(WireErrorEventArgs args)
        {
            SynchronizationContextSend(delegate
            {
                WireError?.Invoke(this, args);
            });
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
                KeyValuePair<string, T> prop;
                lock (this)
                {
                    prop = this.FirstOrDefault(i => i.Key == key);
                }
                var isNull = RealtimeInstance.Child(key).IsNull();
                if (prop.Value == null && !isNull)
                {
                    var item = PropertyFactory(key);
                    if (item == null) return;
                    WireValue(key, item, false);
                    lock (this)
                    {
                        Add(key, item);
                    }
                }
                else if (prop.Value != null && isNull)
                {
                    lock (this)
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

    public class FirebasePropertyDictionary : FirebasePropertyDictionary<FirebaseProperty>
    {
        public FirebasePropertyDictionary()
            : base(new Func<string, FirebaseProperty>(key => new FirebaseProperty()))
        {

        }
    }
}
