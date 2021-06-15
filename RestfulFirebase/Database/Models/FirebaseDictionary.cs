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
            if (IsDisposed)
            {
                return;
            }

            lock (this)
            {
                Subscribe(realtimeInstance);

                List<KeyValuePair<string, T>> objs = this.ToList();
                List<string> supPaths = new List<string>();
                foreach (var path in RealtimeInstance.GetSubPaths())
                {
                    var separatedPath = Utils.UrlSeparate(path);
                    var key = separatedPath[0];
                    if (!supPaths.Contains(key)) supPaths.Add(key);
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
                    if (TryAddCore(path, item))
                    {
                        NotifyObserversOfChange();
                    }
                }
            }

            OnRealtimeAttached(new RealtimeInstanceEventArgs(realtimeInstance));
        }

        public void DetachRealtime()
        {
            if (IsDisposed)
            {
                return;
            }

            foreach (var item in this.ToList())
            {
                item.Value.DetachRealtime();
            }

            var args = new RealtimeInstanceEventArgs(RealtimeInstance);

            Unsubscribe();

            OnRealtimeDetached(args);
        }

        protected T ObjectFactory(string key)
        {
            if (IsDisposed)
            {
                return default;
            }

            return itemInitializer.Invoke((key));
        }

        protected void WireValue(string key, T value, bool invokeSetFirst)
        {
            if (IsDisposed)
            {
                return;
            }

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
            if (IsDisposed)
            {
                return false;
            }

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
            if (IsDisposed)
            {
                return false;
            }

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
            if (IsDisposed)
            {
                return false;
            }

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
                DetachRealtime();
                foreach (var item in this.ToList())
                {
                    TryRemoveWithNotification(item.Key, out _);
                    item.Value.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        private void Subscribe(RealtimeInstance realtimeInstance)
        {
            if (IsDisposed)
            {
                return;
            }

            if (HasAttachedRealtime)
            {
                Unsubscribe();
            }

            RealtimeInstance = realtimeInstance;

            if (HasAttachedRealtime)
            {
                RealtimeInstance.DataChanges += RealtimeInstance_DataChanges;
                RealtimeInstance.Error += RealtimeInstance_Error;
                RealtimeInstance.Disposing += RealtimeInstance_Disposing;
            }
        }

        private void Unsubscribe()
        {
            if (IsDisposed)
            {
                return;
            }

            if (HasAttachedRealtime)
            {
                RealtimeInstance.DataChanges -= RealtimeInstance_DataChanges;
                RealtimeInstance.Error -= RealtimeInstance_Error;
                RealtimeInstance.Disposing -= RealtimeInstance_Disposing;
            }

            RealtimeInstance = null;
        }

        private void RealtimeInstance_DataChanges(object sender, DataChangesEventArgs e)
        {
            if (IsDisposed)
            {
                return;
            }

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
                        if (item == null)
                        {
                            return;
                        }
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
            if (IsDisposed)
            {
                return;
            }

            OnWireError(e);
        }

        private void RealtimeInstance_Disposing(object sender, EventArgs e)
        {
            if (IsDisposed)
            {
                return;
            }

            DetachRealtime();
        }

        #endregion
    }
}
