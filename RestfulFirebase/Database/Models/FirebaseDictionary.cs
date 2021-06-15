using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ObservableHelpers;
using RestfulFirebase.Database.Realtime;
using RestfulFirebase.Extensions;

namespace RestfulFirebase.Database.Models
{
    public class FirebaseDictionary<T> : ObservableDictionary<string, T>, IRealtimeModel
        where T : IRealtimeModel
    {
        #region Properties

        public RealtimeInstance RealtimeInstance { get; private set; }

        public bool HasAttachedRealtime { get => !(RealtimeInstance?.IsDisposed ?? true); }

        public event EventHandler<RealtimeInstanceEventArgs> RealtimeAttached;
        public event EventHandler<RealtimeInstanceEventArgs> RealtimeDetached;
        public event EventHandler<WireErrorEventArgs> WireError;

        private Func<string, T> itemInitializer;

        #endregion

        #region Initializer

        public FirebaseDictionary(Func<string, T> itemInitializer)
        {
            this.itemInitializer = itemInitializer;
        }

        #endregion

        #region Methods

        public void AttachRealtime(RealtimeInstance realtimeInstance, bool invokeSetFirst)
        {
            VerifyNotDisposed();

            if (HasAttachedRealtime)
            {
                Unsubscribe();
                RealtimeInstance = null;
            }

            RealtimeInstance = realtimeInstance;

            Subscribe();

            lock (this)
            {
                List<KeyValuePair<string, T>> objs = this.ToList();
                List<string> supPaths = new List<string>();
                foreach (var path in RealtimeInstance.GetSubPaths())
                {
                    var separatedPath = Utils.UrlSeparate(path);
                    supPaths.Add(separatedPath[0]);
                }

                foreach (var obj in objs)
                {
                    WireValue(obj.Key, obj.Value, invokeSetFirst);
                    supPaths.RemoveAll(i => i == obj.Key);
                }

                foreach (var path in supPaths)
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

        public void DetachRealtime()
        {
            VerifyNotDisposed();

            Unsubscribe();
            var args = new RealtimeInstanceEventArgs(RealtimeInstance);
            RealtimeInstance?.Dispose();
            RealtimeInstance = null;

            OnRealtimeDetached(args);
        }

        protected T ObjectFactory(string key)
        {
            VerifyNotDisposed();

            return itemInitializer.Invoke((key));
        }

        protected void WireValue(string key, T value, bool invokeSetFirst)
        {
            VerifyNotDisposed();

            if (invokeSetFirst) RealtimeInstance.Child(key).PutModel(value);
            else RealtimeInstance.Child(key).SubModel(value);
        }

        protected virtual void OnRealtimeAttached(RealtimeInstanceEventArgs args)
        {
            ContextPost(delegate
            {
                RealtimeAttached?.Invoke(this, args);
            });
        }

        protected virtual void OnRealtimeDetached(RealtimeInstanceEventArgs args)
        {
            ContextPost(delegate
            {
                RealtimeDetached?.Invoke(this, args);
            });
        }

        protected virtual void OnWireError(WireErrorEventArgs args)
        {
            ContextPost(delegate
            {
                WireError?.Invoke(this, args);
            });
        }

        protected override bool ValidateSetItem(string key, T value)
        {
            VerifyNotDisposed();

            var baseValidation = base.ValidateSetItem(key, value);

            if (baseValidation)
            {
                if (HasAttachedRealtime)
                {
                    WireValue(key, value, true);
                }
            }

            return baseValidation;
        }

        protected override bool ValidateRemoveItem(string key)
        {
            VerifyNotDisposed();

            var baseValidation = base.ValidateRemoveItem(key);

            if (baseValidation)
            {
                if (TryGetValueCore(key, out T value))
                {
                    if (!value.IsNull()) value.SetNull();
                    value.Dispose();
                    return true;
                }
                else
                {
                    return false;
                }
            }

            return baseValidation;
        }

        protected override bool ValidateClear()
        {
            VerifyNotDisposed();

            var baseValidation = base.ValidateClear();

            if (baseValidation)
            {
                foreach (var item in this.ToList())
                {
                    ValidateRemoveItem(item.Key);
                }
            }

            return baseValidation;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                foreach (var item in this.ToList())
                {
                    item.Value.Dispose();
                }
                DetachRealtime();
            }
            base.Dispose(disposing);
        }

        private void Subscribe()
        {
            VerifyNotDisposed();

            if (HasAttachedRealtime)
            {
                RealtimeInstance.DataChanges += RealtimeInstance_DataChanges;
                RealtimeInstance.Error += RealtimeInstance_Error;
            }
        }

        private void Unsubscribe()
        {
            VerifyNotDisposed();

            if (HasAttachedRealtime)
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
                    var hasChild = RealtimeInstance.HasChild(key);
                    if (obj.Value == null && hasChild)
                    {
                        var item = ObjectFactory(key);
                        if (item == null) return;
                        WireValue(key, item, false);
                        if (TryAddCore(key, item))
                        {
                            NotifyObserversOfChange();
                        }
                    }
                    else if (obj.Value != null && !hasChild)
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
}
